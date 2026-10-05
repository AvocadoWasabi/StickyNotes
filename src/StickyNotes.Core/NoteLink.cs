namespace StickyNotes.Core;

public static class NoteLink
{
    public static NotePlacement Prepare(FileSnapshot source, string heading, bool daily, string backupDirectory)
    {
        heading = heading.Trim();
        var updated = SectionEditor.EnsureHeading(source.Text, heading);
        if (updated != source.Text) NoteStore.Save(source, updated, backupDirectory);
        else if (NoteStore.Read(source.Path).Hash != source.Hash)
            throw new ConflictException("元ノートが変更されています。再読込してから表示してください。");
        return new NotePlacement { Path = daily ? "" : source.Path, Heading = heading, Daily = daily, Color = "green" };
    }
}
