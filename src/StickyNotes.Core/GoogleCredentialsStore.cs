namespace StickyNotes.Core;

// Only the application's fixed copy is writable/deletable; the selected source is read-only.
public sealed class GoogleCredentialsStore(string directory)
{
    public string FilePath { get; } = Path.Combine(Path.GetFullPath(directory), "credentials-google.json");

    public bool IsManagedPath(string path) => !string.IsNullOrWhiteSpace(path) &&
        Path.IsPathFullyQualified(path) && string.Equals(Path.GetFullPath(path), FilePath, StringComparison.OrdinalIgnoreCase);

    public void Import(string source, Action<string> savePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        CheckTarget();
        var temporary = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            // Validate the exact snapshot that will be installed, not a second read of the source.
            File.Copy(source, temporary);
            CalendarService.ValidateCredentials(temporary);
            Change(() => File.Move(temporary, FilePath, true), () => savePath(FilePath));
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    public void Delete(Action clearPath)
    {
        CheckTarget();
        Change(() => File.Delete(FilePath), clearPath);
    }

    private void CheckTarget()
    {
        foreach (var path in new[] { Path.GetDirectoryName(FilePath)!, FilePath })
            if (Path.Exists(path) && (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new IOException("認証JSONの保存先にリンクは使用できません。");
    }

    private void Change(Action changeFile, Action saveSettings)
    {
        var previous = File.Exists(FilePath) ? File.ReadAllBytes(FilePath) : null;
        changeFile();
        try { saveSettings(); }
        catch
        {
            if (previous is null) File.Delete(FilePath);
            else
            {
                var recovery = FilePath + "." + Guid.NewGuid().ToString("N") + ".tmp";
                // If restoring fails, retain the recovery file and report its location.
                try
                {
                    File.WriteAllBytes(recovery, previous);
                    File.Move(recovery, FilePath, true);
                }
                catch (Exception ex)
                { throw new IOException($"設定保存と認証JSONの復元に失敗しました。復元用ファイルを確認してください: {recovery}", ex); }
            }
            throw;
        }
    }
}
