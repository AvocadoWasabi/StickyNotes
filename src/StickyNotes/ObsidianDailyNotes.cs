using System.Globalization;
using System.Text.Json;

namespace StickyNotes;

internal sealed record ObsidianDailyTarget(string Date, string Path, bool Exists);
internal sealed record ObsidianDailyNotes(string Folder, string Format, string Template, string Configuration, ObsidianDailyTarget[] Targets)
{
    internal static string DateKey(DateTime date) => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    internal string Resolve(DateTime date)
    {
        var target = Targets.Single(t => t.Date == DateKey(date));
        return target.Exists ? target.Path : throw new DailyNoteMissingException();
    }

    internal static ObsidianDailyNotes Decode(JsonElement json, string root, DateTime today)
    {
        var result = json.Deserialize<ObsidianDailyNotes>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        if (result is null || result.Folder is null || string.IsNullOrWhiteSpace(result.Format) || result.Template is null ||
            string.IsNullOrWhiteSpace(result.Configuration) || result.Targets is not { Length: 2 } ||
            !result.Targets.Select(t => t?.Date).Order().SequenceEqual(new[] { DateKey(today), DateKey(today.AddDays(-1)) }.Order()))
            throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        var targets = result.Targets.Select(target =>
        {
            if (string.IsNullOrWhiteSpace(target.Path) || Path.IsPathRooted(target.Path))
                throw new InvalidOperationException(L10n.Text("TasksPreview.OutsideVault"));
            var path = Path.GetFullPath(Path.Combine(root, target.Path));
            _ = ObsidianTasksClient.RelativeNotePath(root, path);
            return target with { Path = path };
        }).ToArray();
        return result with { Targets = targets };
    }
}
