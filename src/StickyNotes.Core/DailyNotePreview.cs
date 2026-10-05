using System.Text;

namespace StickyNotes.Core;

public sealed record DailyNotePreview(string Path, string Text, bool Truncated)
{
    public const int CharacterLimit = 4000;

    public static DailyNotePreview Read(string folder, string pattern, bool useRegex, DateTime today, CancellationToken cancellationToken = default)
    {
        var path = DailyNoteResolver.Resolve(folder, pattern, useRegex, today, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var reader = new StreamReader(stream, new UTF8Encoding(false, true), detectEncodingFromByteOrderMarks: false);
        if (reader.Peek() == '\uFEFF') reader.Read();
        var buffer = new char[CharacterLimit + 1];
        var read = reader.ReadBlock(buffer, 0, buffer.Length);
        cancellationToken.ThrowIfCancellationRequested();
        var length = Math.Min(read, CharacterLimit);
        if (length > 0 && char.IsHighSurrogate(buffer[length - 1])) length--;
        return new(path, new string(buffer, 0, length), read > length);
    }
}
