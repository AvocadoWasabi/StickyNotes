using System.Text.Json;
using System.Text.RegularExpressions;

namespace StickyNotes;

internal static class NoteRename
{
    internal static string Target(string path, string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name != name.Trim() || name.StartsWith('.') || name.EndsWith('.') ||
            name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.Length > 240 ||
            Regex.IsMatch(name, @"^(CON|PRN|AUX|NUL|COM[1-9¹²³]|LPT[1-9¹²³])(?:\.|$)", RegexOptions.IgnoreCase))
            throw new InvalidOperationException(L10n.Text("Rename.Invalid"));
        if (!Path.GetExtension(path).Equals(".md", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(L10n.Text("Rename.Invalid"));
        return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, name + Path.GetExtension(path));
    }

    internal static async Task ApplyAsync(NoteSource source, FileSnapshot expected, string target,
        Action<IReadOnlyDictionary<string, string>> commit, string journalDirectory)
    {
        if (string.Equals(expected.Path, target, StringComparison.Ordinal)) return;
        // Case-only changes are explicitly rejected instead of accidentally overwriting on Windows.
        if (string.Equals(expected.Path, target, StringComparison.OrdinalIgnoreCase) || await source.PathExistsAsync(target))
            throw new IOException(L10n.Text("StickyFolder.Conflict"));
        Directory.CreateDirectory(journalDirectory);
        var operation = Guid.NewGuid().ToString("N");
        var journal = Path.Combine(journalDirectory, operation + ".json");
        File.WriteAllText(journal, JsonSerializer.Serialize(new { From = expected.Path, To = target, expected.Hash, Operation = operation }));
        try
        {
            await source.RenameAsync(expected, target, operation);
            commit(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { [expected.Path] = target });
        }
        catch (Exception error)
        {
            // Native link updates can change other files. Never undo those with a raw filesystem move.
            throw new IOException(L10n.Text("Rename.Recovery") + "\n" + expected.Path + " → " + target + "\n" + journal + "\n" + error.Message, error);
        }
        try { File.Delete(journal); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }
}
