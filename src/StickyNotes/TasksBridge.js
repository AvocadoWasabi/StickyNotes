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
    stickyRequireTasksVersion(plugin);
    if (plugin.getState?.() !== 'Warm') fail('Tasks is still loading. Retry after its index is ready.');
    if (typeof plugin.queryRenderer?.addQueryRenderChild !== 'function')
        fail('STICKY_TASKS_UNSUPPORTED_API');
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
                fail('STICKY_TASKS_UNSUPPORTED_API');
            if (renderer.query.error) fail(String(renderer.query.error));
            renderer.performSearch(plugin.getTasks());
            const targets = stickyTaskTargets();
            // 8.2.2 creates its Markdown renderer inside resultsAsMarkdown().
            // Decorate only the detached, already-filtered result: never the cache,
            // query inputs or shared prototypes. Preserve identity across tree/group references.
            const result = renderer.filteredQueryResult;
            if (!Array.isArray(result?.taskGroups?.groups)) fail('STICKY_TASKS_UNSUPPORTED_API');
            if (result.searchErrorMessage) fail(String(result.searchErrorMessage));
            const copies = new WeakMap();
            const wrap = item => {
                if (item == null) return item;
                if (copies.has(item)) return copies.get(item);
                if (typeof item.isTask !== 'boolean' || !Array.isArray(item.children) ||
                    typeof item.description !== 'string') fail('STICKY_TASKS_UNSUPPORTED_API');
                const copy = Object.create(item);
                copies.set(item, copy);
                const mark = text => targets.mark(text, {path:item.taskLocation?.path,
                    line:item.taskLocation?.lineNumber, original:item.originalMarkdown});
                Object.defineProperties(copy, {
                    parent: {get: () => wrap(item.parent)},
                    children: {get: () => item.children.map(wrap)},
                    ...(item.isTask ? {toString: {value: () => mark(item.toString())}} :
                        {description: {get: () => mark(item.description)}})
                });
                return copy;
            };
            const groups = result.taskGroups.groups.map(group => {
                if (!Array.isArray(group.tasks)) fail('STICKY_TASKS_UNSUPPORTED_API');
                return Object.create(group, {tasks: {value: group.tasks.map(wrap)}});
            });
            renderer.filteredQueryResult = Object.create(result, {
                taskGroups: {value: Object.create(result.taskGroups, {groups: {value: groups}})}
            });
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
