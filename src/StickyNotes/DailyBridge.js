// Resolve dates with the same native path routine used by Obsidian daily:path.
// Never call getDailyNote: it creates a missing note.
async function stickyTasksPreview(request) {
    const fail = message => { throw new Error(message); };
    const normalize = value => value.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(request.root))
        fail('The selected Obsidian vault does not match the configured vault folder.');
    const plugin = app.internalPlugins.plugins['daily-notes'];
    if (!plugin?.enabled) fail('STICKY_DAILY_DISABLED');
    const daily = plugin.instance;
    if (typeof daily?.getDailyNotePathBeforeAppLoads !== 'function' || typeof daily?.getFormat !== 'function')
        fail('STICKY_DAILY_UNSUPPORTED');
    const folder = daily.options.folder || '';
    const format = daily.getFormat();
    const template = daily.options.template || '';
    const configuration = JSON.stringify([request.root, folder, format, app.vault.getConfig('newFileLocation'), app.vault.getConfig('newFileFolderPath')]);
    if (!Array.isArray(request.dates) || request.dates.length !== 2) fail('Invalid dates.');
    const targets = [];
    for (const date of request.dates) {
        const value = window.moment(date, 'YYYY-MM-DD', true);
        if (!value.isValid()) fail('Invalid date.');
        const path = await daily.getDailyNotePathBeforeAppLoads(value);
        if (!path) fail('STICKY_DAILY_PATH');
        const file = app.vault.getAbstractFileByPathInsensitive(path);
        targets.push({ date, path: file?.path || path, exists: !!file && file.extension === 'md' });
    }
    return { folder, format, template, configuration, targets };
}
