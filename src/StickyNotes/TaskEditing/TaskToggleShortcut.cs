using System.Windows.Input;

namespace StickyNotes;

internal static class TaskToggleShortcut
{
    internal static bool IsAllowed(Key key, ModifierKeys modifiers)
    {
        if ((modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) == 0 ||
            (modifiers & ~(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift)) != 0) return false;
        if (key is not (>= Key.A and <= Key.Z or >= Key.D0 and <= Key.D9 or >= Key.F1 and <= Key.F12 or Key.Enter or Key.Space)) return false;
        // Preserve note commands, standard text editing and window closing.
        if (modifiers == ModifierKeys.Control && key is Key.A or Key.C or Key.X or Key.V or Key.Z or Key.Y or Key.S or Key.E) return false;
        if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Z) return false;
        return (modifiers & ModifierKeys.Alt) == 0 || key != Key.F4;
    }

    internal static string Format(Key key, ModifierKeys modifiers) =>
        new KeyGestureConverter().ConvertToInvariantString(new KeyGesture(key, modifiers))!;

    internal static KeyGesture? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 80) return null;
        try
        {
            var gesture = new KeyGestureConverter().ConvertFromInvariantString(value) as KeyGesture;
            return gesture is not null && IsAllowed(gesture.Key, gesture.Modifiers) ? gesture : null;
        }
        catch (Exception ex) when (ex is FormatException or NotSupportedException or ArgumentException) { return null; }
    }

    internal static bool Matches(string? value, Key key, ModifierKeys modifiers) =>
        Parse(value) is { } gesture && gesture.Key == key && gesture.Modifiers == modifiers;
}
