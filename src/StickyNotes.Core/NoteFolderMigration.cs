namespace StickyNotes.Core;

public static class NoteFolderMigration
{
    public static string Normalize(string path)
    {
        if (!Path.IsPathFullyQualified(path)) throw new InvalidOperationException("保存フォルダは絶対パスで指定してください。");
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    public static bool SameFolder(string first, string second) =>
        string.Equals(Normalize(first), Normalize(second), StringComparison.OrdinalIgnoreCase);

    public static bool Contains(string folder, string path)
    {
        var root = Normalize(folder);
        if (!Path.EndsInDirectorySeparator(root)) root += Path.DirectorySeparatorChar;
        return Normalize(path).StartsWith(root, StringComparison.OrdinalIgnoreCase);
    }

    // Reject links/junctions rather than traversing outside the selected folders.
    private static void CheckPath(string path)
    {
        for (string? current = path; current is not null; current = Path.GetDirectoryName(current))
        {
            if ((File.Exists(current) || Directory.Exists(current)) &&
                (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("リンク・ジャンクションを含むパスは移行できません: " + current);
        }
    }

    public static void Move(string source, string destination, Action<IReadOnlyDictionary<string, string>> saveSettings)
    {
        source = Normalize(source); destination = Normalize(destination);
        var paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (SameFolder(source, destination)) { saveSettings(paths); return; }
        if (Contains(source, destination) || Contains(destination, source))
            throw new IOException("移行元と移行先には、互いに親子関係にないフォルダを指定してください。");
        CheckPath(source); CheckPath(destination);
        if (File.Exists(source)) throw new IOException("移行元はフォルダではありません: " + source);
        if (Directory.Exists(source)) Collect(source);

        void Collect(string folder)
        {
            foreach (var file in Directory.EnumerateFiles(folder).Order(StringComparer.OrdinalIgnoreCase))
            {
                if (!Path.GetExtension(file).Equals(".md", StringComparison.OrdinalIgnoreCase)) continue;
                CheckPath(file);
                var target = Path.Combine(destination, Path.GetRelativePath(source, file));
                CheckPath(target);
                if (File.Exists(target) || Directory.Exists(target)) throw new IOException("移行先に同名のファイルまたはフォルダがあります。上書きしません: " + target);
                paths.Add(file, target);
            }
            foreach (var child in Directory.EnumerateDirectories(folder).Order(StringComparer.OrdinalIgnoreCase))
            {
                CheckPath(child);
                Collect(child);
            }
        }

        var moved = new List<KeyValuePair<string, string>>();
        try
        {
            Directory.CreateDirectory(destination);
            foreach (var pair in paths)
            {
                CheckPath(pair.Key); CheckPath(pair.Value);
                Directory.CreateDirectory(Path.GetDirectoryName(pair.Value)!);
                File.Move(pair.Key, pair.Value); // Never overwrite, including files created after preflight.
                moved.Add(pair);
            }
            saveSettings(paths);
        }
        catch (Exception error)
        {
            var remaining = new List<string>();
            foreach (var pair in moved.AsEnumerable().Reverse())
            {
                try { CheckPath(pair.Value); CheckPath(pair.Key); File.Move(pair.Value, pair.Key); }
                catch (Exception rollbackError) { remaining.Add($"{pair.Value} → {pair.Key}: {rollbackError.Message}"); }
            }
            if (remaining.Count > 0)
                throw new IOException("移行に失敗し、一部のファイルを元に戻せませんでした。次の移行先と元の保存先を確認してください。ファイルは上書きしていません。\n" + string.Join("\n", remaining) + "\n原因: " + error.Message, error);
            throw;
        }
    }
}
