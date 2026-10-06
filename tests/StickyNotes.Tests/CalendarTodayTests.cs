using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows.Controls;
using StickyNotes;
using StickyNotes.Core;

internal static partial class Program
{
    private static void CalendarTodayTests(string root)
    {
        foreach (var date in new[] { new DateTime(2026, 12, 31, 23, 59, 0), new DateTime(2028, 2, 29, 12, 0, 0) })
        {
            var query = CalendarQuery.Parse("@CALENDAR ToDaY 設計 会議", date);
            Check(query.From == new DateTimeOffset(DateTime.SpecifyKind(date.Date, DateTimeKind.Local)) &&
                query.Until == new DateTimeOffset(DateTime.SpecifyKind(date.Date.AddDays(1), DateTimeKind.Local)) &&
                query.Search == "設計 会議", "today resolves local midnights across month/year boundaries: " + date);
        }
        Check(CalendarQuery.Parse("@calendar today").From.Date == DateTime.Today, "today defaults to the PC date");
        Throws<FormatException>(() => CalendarQuery.Parse("@calendar todayX"), "today must be a complete token");
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ar-SA");
            Check(CalendarQuery.Parse("@calendar today", new DateTime(2026, 10, 7)).From.Year == 2026,
                "today is independent of regional calendar settings");
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = culture; }

        var app = App.Current;
        var previousService = app.Calendar;
        var previousCredentials = app.Config.GoogleCredentialsFile;
        var credentials = Path.Combine(root, "today-oauth.json");
        File.WriteAllText(credentials, "{\"installed\":{\"client_id\":\"today-test\"}}");
        var stored = JsonSerializer.Serialize(new { clientId = "today-test", tokens = new OAuthTokens
            { AccessToken = "fake-token", ExpiresAt = DateTimeOffset.UtcNow.AddHours(1) } });
        var handler = new FakeHandler();
        using var service = new CalendarService(() => stored, _ => { }, handler);
        var calendarProperty = typeof(App).GetProperty(nameof(App.Calendar))!;
        calendarProperty.SetValue(app, service);
        app.Config.GoogleCredentialsFile = credentials;
        var path = Path.Combine(root, "today-note.md");
        const string content = "@calendar today";
        File.WriteAllText(path, content);
        var day = new DateTime(2026, 12, 31);
        var window = new NoteWindow(new NotePlacement { Path = path }, null, null, () => day);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        void Call(string name) => typeof(NoteWindow).GetMethod(name, flags)!.Invoke(window, null);
        void Tick() => ((Task)typeof(NoteWindow).GetMethod("Tick", flags)!.Invoke(window, null)!).GetAwaiter().GetResult();
        void Set(string name, object? value) => typeof(NoteWindow).GetField(name, flags)!.SetValue(window, value);
        void Due() => Set("lastCalendarCheck", Environment.TickCount64 - 60_001);
        var panel = (StackPanel)typeof(NoteWindow).GetField("eventsPanel", flags)!.GetValue(window)!;
        DateTime RequestedDay() => DateTimeOffset.Parse(System.Web.HttpUtility.ParseQueryString(new Uri(handler.Uri!).Query)["timeMin"]!,
            System.Globalization.CultureInfo.InvariantCulture).Date;
        try
        {
            Call("Reload"); Tick();
            Check(handler.Requests == 1 && panel.Children.Count == 2 && RequestedDay() == day,
                "today note fetches and displays events on first poll");
            Tick(); Tick();
            Check(handler.Requests == 1, "two-second note polling does not repeatedly request calendar data");
            day = day.AddDays(1); Due(); Tick();
            Check(handler.Requests == 2 && RequestedDay() == day && File.ReadAllText(path) == content,
                "next refresh follows midnight without rewriting the saved today command");
            day = day.AddDays(-2); Due(); Tick();
            Check(handler.Requests == 3 && RequestedDay() == day, "backward PC date correction still refreshes");
            day = day.AddDays(4); handler.Conflict = true; Due(); Tick();
            Check(panel.Children.Count == 0, "failed retrieval for a new day clears the old day's events");
            var requests = handler.Requests;
            Tick();
            Check(handler.Requests == requests, "failed retrieval retains the normal retry interval");
            handler.Conflict = false; Due(); Tick();
            Check(panel.Children.Count == 2 && RequestedDay() == day, "retry displays the current day after an error");
            handler.OnRequest = () => { day = day.AddDays(1); handler.OnRequest = null; };
            Due(); Tick();
            Check(panel.Children.Count == 0, "response crossing midnight is discarded");
            Tick();
            Check(panel.Children.Count == 2 && RequestedDay() == day, "discarded response is replaced on the next poll");
            requests = handler.Requests;
            Set("editing", true); Due(); Tick();
            Check(handler.Requests == requests, "automatic calendar refresh pauses during note editing");
            Set("editing", false); Tick();
            Check(handler.Requests == requests + 1, "calendar refresh resumes after editing");
            Set("closed", true); Due(); Tick();
            Check(handler.Requests == requests + 1, "closed notes do not refresh");
        }
        finally
        {
            window.Close();
            calendarProperty.SetValue(app, previousService);
            app.Config.GoogleCredentialsFile = previousCredentials;
        }
    }
}
