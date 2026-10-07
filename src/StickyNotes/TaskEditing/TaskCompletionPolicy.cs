using System.Threading;

namespace StickyNotes;

internal static class TaskCompletionPolicy
{
    internal static async Task<bool> IsEnabledAsync(Settings settings, Func<Settings, NoteSource> source, CancellationToken token)
    {
        if (settings.TaskCompletion == TaskCompletionMode.On) return true;
        if (settings.TaskCompletion != TaskCompletionMode.Automatic || !NoteSource.UsesCli(settings)) return false;
        // Unknown/disabled/unreachable plugins are not permission to enable completion.
        try { return await source(settings).HasTasksPluginAsync(token).ConfigureAwait(false); }
        catch (Exception) { return false; }
    }
}
