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
    if (typeof plugin.api?.query !== 'function' || typeof plugin.api?.markdownTaskList !== 'function' || typeof plugin.index?.initialized !== 'boolean')
        fail('STICKY_DATAVIEW_UnsupportedApi');
    if (!plugin.index.initialized) fail('STICKY_DATAVIEW_Loading');
    const results = [];
    for (const query of request.queries) {
        try {
            if (/^\s*CALENDAR\b/i.test(query)) fail('STICKY_DATAVIEW_UnsupportedQuery');
            const result = await plugin.api.query(query, request.path);
            if (result?.successful === false) fail(String(result.error || 'STICKY_DATAVIEW_InvalidResponse'));
            if (result?.successful !== true || !result.value) fail('STICKY_DATAVIEW_InvalidResponse');
            const targets = stickyTaskTargets(), data = result.value;
            const decorate = items => items.map(item => Array.isArray(item.rows) ? {...item, rows:decorate(item.rows)} :
                {...item, children:decorate(item.children || []), visual:item.task ?
                    targets.mark(item.visual ?? item.text, {path:item.path,line:item.line,status:item.status,text:item.text}) : item.visual});
            const markdown = data.type === 'task' ? plugin.api.markdownTaskList(decorate(data.values)) :
                data.type === 'list' ? plugin.api.markdownList(data.values) :
                data.type === 'table' ? plugin.api.markdownTable(data.headers, data.values) : null;
            if (typeof markdown !== 'string') fail('STICKY_DATAVIEW_InvalidResponse');
            if (markdown.length > 240000) fail('STICKY_DATAVIEW_OutputLimit');
            const exported = await targets.finish(markdown);
            if (exported.markdown.length > 200000) fail('STICKY_DATAVIEW_OutputLimit');
            results.push(exported);
        } catch (error) {
            results.push({ markdown: null, error: String(error.message || error).slice(0, 4000) });
        }
    }
    return { version: plugin.manifest.version, results };
}
