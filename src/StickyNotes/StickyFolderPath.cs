namespace StickyNotes;

internal static class StickyFolderPath
{
    internal static string Normalize(string value)
    {
        value = value.Trim().Replace('\\', '/');
        if (value is "" or ".") return ".";
        var parts = value.Split('/');
        if (Path.IsPathRooted(value) || parts.Any(p => p.Length == 0 || p.StartsWith('.') || p.EndsWith('.') || p.EndsWith(' ') ||
            p.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            System.Text.RegularExpressions.Regex.IsMatch(p, @"^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase)))
            throw new InvalidOperationException(L10n.Text("StickyFolder.Invalid"));
        return string.Join('/', parts);
    }

    internal static string Absolute(string root, string relative)
    {
        root = NoteFolderMigration.Normalize(root);
        relative = Normalize(relative);
        return relative == "." ? root : Path.GetFullPath(Path.Combine(root, relative));
    }

    internal static string Relative(string root, string path) => Normalize(Path.GetRelativePath(root, path));
}
