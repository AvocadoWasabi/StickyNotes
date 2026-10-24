// Shared by query export and both native checkbox paths.
function stickyRequireTasksVersion(plugin) {
    const version = String(plugin?.manifest?.version ?? 'unknown');
    const match = /^(\d+)\.(\d+)\.(\d+)(?:\+[0-9A-Za-z.-]+)?$/.exec(version);
    const parts = match?.slice(1).map(Number);
    if (!parts || !(parts[0] > 8 || parts[0] === 8 && (parts[1] > 2 || parts[1] === 2 && parts[2] >= 2)))
        throw new Error('STICKY_TASKS_UPDATE_REQUIRED:' + version.slice(0, 80));
}
