using System.Text;
using System.Threading;

namespace StickyNotes;

internal sealed partial class NoteSource
{
    internal async Task<bool> HasTasksPluginAsync(CancellationToken token)
    {
        if (!cli) return false;
        using var command = ObsidianTasksClient.PrepareTaskCheckboxCommand(settings, new { root = Root, mode = "availability" });
        using var response = ObsidianTasksClient.ParseEnvelope(await execute(settings, command.Code, token).ConfigureAwait(false));
        return response.RootElement.GetProperty("available").GetBoolean();
    }

    // Return an edited body/section. SaveContent remains the single authority for persisting it.
    internal string ToggleTaskContent(FileSnapshot expected, string content, int line, bool value)
    {
        var fallback = SectionEditor.ToggleTaskAtLine(content, line, value); // Validate before contacting Obsidian.
        if (!cli || fallback == content) return fallback;
        if (!expected.Hash.StartsWith("cli:", StringComparison.Ordinal))
            throw new ConflictException(L10n.Text("CliNote.ReloadBeforeSave"));
        var lines = content.Split('\n');
        var original = lines[line].TrimEnd('\r');
        var replacement = Task.Run(async () =>
        {
            var path = ObsidianTasksClient.RelativeNotePath(Root, expected.Path);
            using var command = ObsidianTasksClient.PrepareTaskCheckboxCommand(settings, new { root = Root, path, line = original, @checked = value });
            using var response = ObsidianTasksClient.ParseEnvelope(await execute(settings, command.Code, CancellationToken.None).ConfigureAwait(false));
            return response.RootElement.GetProperty("text").GetString();
        }).GetAwaiter().GetResult();
        if (replacement is null || Encoding.UTF8.GetByteCount(replacement) > 2_000_000)
            throw new InvalidOperationException(L10n.Text("TasksPreview.NoResponse"));
        var start = 0;
        for (var i = 0; i < line; i++) start += lines[i].Length + 1;
        if (replacement.Length == 0) // Tasks 'on completion: delete' removes the complete line.
            return content.Remove(start, lines[line].Length + (line < lines.Length - 1 ? 1 : 0));
        var newline = content.Contains("\r\n") ? "\r\n" : "\n";
        replacement = replacement.Replace("\r\n", "\n").Replace("\n", newline);
        return content[..start] + replacement + content[(start + original.Length)..];
    }
}
