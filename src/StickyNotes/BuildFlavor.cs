namespace StickyNotes;

internal static class BuildFlavor
{
#if TASKS_PREVIEW
    internal static bool TasksPreview => true;
#else
    internal static bool TasksPreview => false;
#endif
    internal static string Profile => TasksPreview ? "StickyNotes-TasksPreview" : "StickyNotes";
}
