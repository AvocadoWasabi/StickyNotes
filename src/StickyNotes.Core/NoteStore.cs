using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace StickyNotes.Core;

public static class NoteStore
{
    public static FileSnapshot Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var text = new UTF8Encoding(false, true).GetString(bytes);
        return new(path, text.TrimStart('\uFEFF'), Convert.ToHexString(SHA256.HashData(bytes)));
    }

    // Recheck while holding a write handle; FileShare.Read prevents cooperating writers.
    public static FileSnapshot Save(FileSnapshot expected, string text, string? backupDirectory = null)
    {
        using var file = new FileStream(expected.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.Read);
        using var memory = new MemoryStream();
        file.CopyTo(memory);
        var previous = memory.ToArray();
        if (Convert.ToHexString(SHA256.HashData(previous)) != expected.Hash)
            throw new ConflictException(L10n.Text("NoteStore.Text01"));
        var backups = backupDirectory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "StickyNotes", "backups");
        Directory.CreateDirectory(backups);
        File.WriteAllBytes(Path.Combine(backups, $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.md"), previous);
        var bytes = new UTF8Encoding(previous.AsSpan().StartsWith(new byte[] { 239, 187, 191 })).GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(text)).ToArray();
        try
        {
            file.Position = 0;
            file.Write(bytes);
            file.SetLength(bytes.Length);
            file.Flush(true);
        }
        catch
        {
            file.Position = 0;
            file.Write(previous);
            file.SetLength(previous.Length);
            file.Flush(true);
            throw;
        }
        return new(expected.Path, text, Convert.ToHexString(SHA256.HashData(bytes)));
    }

    public static (string Frontmatter, string Body) Split(string text)
    {
        var match = Regex.Match(text, @"\A---\r?\n(?<yaml>[\s\S]*?)\r?\n---(?:\r?\n|$)");
        return match.Success ? (match.Groups["yaml"].Value, text[match.Length..]) : ("", text);
    }

    private static YamlMappingNode Parse(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml)) return new();
        var stream = new YamlStream();
        stream.Load(new StringReader(yaml));
        return stream.Documents[0].RootNode as YamlMappingNode ?? throw new FormatException(L10n.Text("NoteStore.Text02"));
    }

    public static NoteMetadata Metadata(string text)
    {
        var root = Parse(Split(text).Frontmatter);
        string Get(string key, string fallback) => root.Children.TryGetValue(new YamlScalarNode(key), out var n) ? n.ToString() : fallback;
        var tags = root.Children.TryGetValue(new YamlScalarNode("tags"), out var t)
            ? t is YamlSequenceNode seq ? seq.Children.Select(x => x.ToString()).ToArray() : t.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries)
            : [];
        return new(Get("title", L10n.Text("NoteStore.Text03")), tags, Get("status", "active"), Get("color", "yellow"));
    }

    public static string WithMetadata(string text, NoteMetadata metadata)
    {
        var (yaml, body) = Split(text);
        var root = Parse(yaml);
        void Set(string key, YamlNode value) => root.Children[new YamlScalarNode(key)] = value;
        Set("title", new YamlScalarNode(metadata.Title));
        Set("tags", new YamlSequenceNode(metadata.Tags.Select(t => new YamlScalarNode(t.TrimStart('#')))));
        Set("status", new YamlScalarNode(metadata.Status));
        Set("color", new YamlScalarNode(metadata.Color));
        Set("updated", new YamlScalarNode(DateTimeOffset.Now.ToString("o")));
        using var writer = new StringWriter();
        new YamlStream(new YamlDocument(root)).Save(writer, false);
        var serialized = Regex.Replace(writer.ToString(), @"\r?\n\.\.\.\r?\n?$", "").TrimEnd();
        return $"---\n{serialized}\n---\n{body}";
    }

    public static string Create(string directory)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, L10n.Format("NoteStore.Text04", DateTime.Now, Guid.NewGuid().ToString("N")[..6]));
        File.WriteAllText(path, InitialText(), new UTF8Encoding(false));
        return path;
    }

    public static string InitialText()
    {
        var text = L10n.Format("NoteStore.Text05", Guid.NewGuid(), DateTimeOffset.Now);
        return WithMetadata(text, new(L10n.Text("NoteStore.Text06"), ["sticky"], "active", "yellow"));
    }
}
