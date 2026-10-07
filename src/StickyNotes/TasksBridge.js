// Executed inside the existing Obsidian instance by the official CLI.
// Input is data, never interpolated JavaScript. No clipboard access.
async function stickyTasksPreview(request) {
    const fail = message => { throw new Error(message); };
    const normalize = value => value.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(request.root))
        fail('The selected Obsidian vault does not match the configured vault folder.');
    const file = app.vault.getAbstractFileByPath(request.path);
    if (!file || file.extension !== 'md') fail('The source Markdown note is not in this vault.');
    const plugin = app.plugins.plugins['obsidian-tasks-plugin'];
    if (!plugin) fail('Enable Tasks in this Obsidian vault.');
    if (plugin.getState?.() !== 'Warm') fail('Tasks is still loading. Retry after its index is ready.');
    if (typeof plugin.queryRenderer?.addQueryRenderChild !== 'function')
        fail('This Tasks version does not expose the preview adapter.');
    const results = [];
    for (const query of request.queries) {
        let child;
        try {
            await plugin.queryRenderer.addQueryRenderChild(query, document.createElement('div'), {
                sourcePath: request.path,
                addChild: value => {
                    child = value;
                    // Only construct the renderer. Do not subscribe a detached preview to
                    // cache events, DOM observers or midnight timers in the user's app.
                    value.load = () => {};
                }
            });
            const renderer = child?.queryResultsRenderer;
            if (typeof renderer?.performSearch !== 'function' || typeof renderer?.resultsAsMarkdown !== 'function')
                fail('This Tasks version does not expose Markdown query results.');
            if (renderer.query.error) fail(String(renderer.query.error));
            renderer.performSearch(plugin.getTasks());
            const targets = stickyTaskTargets();
            const exporter = renderer.markdownRenderer;
            if (typeof exporter?.formatTask !== 'function' || typeof exporter?.formatListItem !== 'function')
                fail('This Tasks version does not expose source task mapping.');
            for (const method of ['formatTask', 'formatListItem']) {
                const original = exporter[method];
                exporter[method] = function(task) {
                    const text = original.call(this, task);
                    return targets.mark(text, {path:task.taskLocation?.path, line:task.taskLocation?.lineNumber, original:task.originalMarkdown});
                };
            }
            const markdown = await renderer.resultsAsMarkdown();
            if (typeof markdown !== 'string' || markdown.length > 260000)
                fail('Query output is too large (200,000 characters). Add a limit to the query.');
            const exported = await targets.finish(markdown);
            if (exported.markdown.length > 200000) fail('Query output is too large (200,000 characters). Add a limit to the query.');
            results.push(exported);
        } catch (error) {
            results.push({ markdown: null, error: String(error.message || error).slice(0, 4000) });
        } finally {
            child?.unload();
        }
    }
    return { version: plugin.manifest.version, results };
}
