// Executed inside Obsidian by the official CLI. DQL stays data; never execute DataviewJS.
async function stickyTasksPreview(request) {
    const fail = message => { throw new Error(message); };
    const normalize = value => value.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(request.root))
        fail('The selected Obsidian vault does not match the configured vault folder.');
    const file = app.vault.getAbstractFileByPath(request.path);
    if (!file || file.extension !== 'md') fail('The source Markdown note is not in this vault.');
    if (!Array.isArray(request.queries) || request.queries.length > 20 ||
        request.queries.some(q => typeof q !== 'string') || request.queries.reduce((n, q) => n + q.length, 0) > 8000)
        fail('STICKY_DATAVIEW_QueryLimit');
    const plugin = app.plugins.plugins.dataview;
    if (!plugin) fail('STICKY_DATAVIEW_MissingPlugin');
    if (typeof plugin.api?.queryMarkdown !== 'function' || typeof plugin.index?.initialized !== 'boolean')
        fail('STICKY_DATAVIEW_UnsupportedApi');
    if (!plugin.index.initialized) fail('STICKY_DATAVIEW_Loading');
    const results = [];
    for (const query of request.queries) {
        try {
            if (/^\s*CALENDAR\b/i.test(query)) fail('STICKY_DATAVIEW_UnsupportedQuery');
            const result = await plugin.api.queryMarkdown(query, request.path);
            if (result?.successful === false) fail(String(result.error || 'STICKY_DATAVIEW_InvalidResponse'));
            if (result?.successful !== true || typeof result.value !== 'string') fail('STICKY_DATAVIEW_InvalidResponse');
            if (result.value.length > 200000) fail('STICKY_DATAVIEW_OutputLimit');
            results.push({ markdown: result.value, error: null });
        } catch (error) {
            results.push({ markdown: null, error: String(error.message || error).slice(0, 4000) });
        }
    }
    return { version: plugin.manifest.version, results };
}
