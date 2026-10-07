// A pure transformation in Obsidian. The existing save path owns backups and conflict checks.
async function stickyTasksPreview(request) {
    const fail = message => { throw new Error(message); };
    const normalize = value => value.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(request.root))
        fail('The selected Obsidian vault does not match the configured vault folder.');
    if (request.mode === 'availability') return { available: !!app.plugins.plugins['obsidian-tasks-plugin'] };
    const file = app.vault.getAbstractFileByPath(request.path);
    if (!file || file.extension !== 'md') fail('The source Markdown note is not in this vault.');
    if (typeof request.line !== 'string' || /[\r\n]/.test(request.line) ||
        Buffer.byteLength(request.line, 'utf8') > 2000000 || typeof request.checked !== 'boolean') fail('Invalid task line.');
    const match = request.line.match(/^([ \t]*(?:[-+*]|\d+[.)])[ \t]+\[)([ xX])(\].*)$/);
    if (!match) fail('Invalid task line.');
    if ((match[2] !== ' ') === request.checked) return { text: request.line };
    const plugin = app.plugins.plugins['obsidian-tasks-plugin'];
    if (!plugin) return { text: match[1] + (request.checked ? 'x' : ' ') + match[3] };
    if (typeof plugin.apiV1?.executeToggleTaskDoneCommand !== 'function')
        fail('Tasks 7.2 or later is required for native checkbox editing.');
    const text = plugin.apiV1.executeToggleTaskDoneCommand(request.line, file.path);
    if (typeof text !== 'string' || Buffer.byteLength(text, 'utf8') > 2000000) fail('Invalid Tasks update.');
    return { text };
}
