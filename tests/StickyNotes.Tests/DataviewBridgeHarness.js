const assert = require('node:assert/strict');
const zlib = require('node:zlib');

async function main() {
    let input = '';
    for await (const chunk of process.stdin) input += chunk;
    const { code } = JSON.parse(input.replace(/^\uFEFF/, ''));
    let count = 0;
    const check = (condition, label) => { assert.ok(condition, label); console.log('PASS: ' + label); count++; };
    const calls = [];
    const plugin = {
        manifest: { version: 'synthetic-api' }, index: { initialized: true },
        api: { query: async (query, origin) => {
            calls.push({ query, origin });
            if (query === 'broken query') return { successful: false, error: 'DQL parse error' };
            if (query === 'oversized') return { successful: true, value: {type:'list',values:['x'.repeat(200001)]} };
            if (query === 'invalid response') return { successful: true, value: {} };
            if (query === 'throws') throw new Error('Plugin error');
            return { successful: true, value: query.startsWith('LIST') ? {type:'list',values:['Entry']} : query.startsWith('TABLE') ? {type:'table',headers:['Name'],values:[['Entry']]} : {type:'task',values:[]} };
        }, markdownList: values => '- ' + values.join('\n- '), markdownTable: () => '| Name |\n| --- |\n| Entry |', markdownTaskList: () => '- [ ] Task' }
    };
    const vault = {
        adapter: { getBasePath: () => 'c:\\SyntheticVault\\' },
        getAbstractFileByPath: path => path === '日本語/note.md' ? { extension: 'md', path } : null
    };
    global.app = { vault, plugins: { plugins: { dataview: plugin } } };
    const run = async (script = code) => JSON.parse((await eval(script)).replace(/^STICKY_TASKS_PREVIEW:/, ''));
    let result = await run();
    check(result.version === 'synthetic-api' && result.results.length === 8, 'Dataview bridge: framed response keeps block order and plugin version');
    check(result.results[0].markdown === '- Entry' && result.results[1].markdown.startsWith('| Name |') && result.results[2].markdown === '- [ ] Task', 'Dataview bridge: LIST/TABLE/TASK Markdown preserved');
    check(calls.every(call => call.origin === '日本語/note.md') && calls[1].query.includes('this.file.path'), 'Dataview bridge: source-note context passed unchanged');
    check(result.results[3].error === 'DQL parse error' && result.results[7].error === 'Plugin error', 'Dataview bridge: malformed and thrown query errors isolated by block');
    check(result.results[4].error === 'STICKY_DATAVIEW_UnsupportedQuery' && !calls.some(call => call.query.startsWith('CALENDAR')), 'Dataview bridge: CALENDAR rejected before API execution');
    check(result.results[5].error === 'STICKY_DATAVIEW_OutputLimit' && result.results[6].error === 'STICKY_DATAVIEW_InvalidResponse', 'Dataview bridge: oversized and invalid output rejected');
    // No write methods, Tasks plugin, JS executor, components or subscriptions exist in this mock.
    check(!Object.hasOwn(vault, 'modify') && calls.length === 7, 'Dataview bridge: works without write methods or Tasks/DataviewJS/render APIs');
    delete app.plugins.plugins.dataview;
    check((await run()).error === 'STICKY_DATAVIEW_MissingPlugin', 'Dataview bridge: missing plugin handled');
    app.plugins.plugins.dataview = plugin;
    plugin.index.initialized = false;
    check((await run()).error === 'STICKY_DATAVIEW_Loading', 'Dataview bridge: indexing is retryable');
    delete plugin.index.initialized;
    check((await run()).error === 'STICKY_DATAVIEW_UnsupportedApi', 'Dataview bridge: unknown index contract rejected');
    plugin.index.initialized = true;
    const queryApi = plugin.api.query; delete plugin.api.query;
    check((await run()).error === 'STICKY_DATAVIEW_UnsupportedApi', 'Dataview bridge: missing query API rejected');
    plugin.api.query = queryApi;
    vault.adapter.getBasePath = () => 'C:/OtherVault';
    check((await run()).error.includes('does not match'), 'Dataview bridge: wrong vault rejected');
    vault.adapter.getBasePath = () => 'C:/SyntheticVault';
    vault.getAbstractFileByPath = () => ({ extension: 'txt' });
    check((await run()).error.includes('source Markdown'), 'Dataview bridge: non-Markdown source rejected');
    vault.getAbstractFileByPath = () => ({ extension: 'md' });
    const payload = code.match(/Buffer.from\('([^']+)'/)[1];
    const request = JSON.parse(zlib.inflateSync(Buffer.from(payload, 'base64')));
    const altered = queries => code.replace(payload, zlib.deflateSync(Buffer.from(JSON.stringify({ ...request, queries }))).toString('base64'));
    check((await run(altered(Array(21).fill('LIST')))).error === 'STICKY_DATAVIEW_QueryLimit', 'Dataview bridge: block count checked inside Obsidian');
    check((await run(altered(['x'.repeat(8001)]))).error === 'STICKY_DATAVIEW_QueryLimit', 'Dataview bridge: query length checked inside Obsidian');
    check((await run(altered([null]))).error === 'STICKY_DATAVIEW_QueryLimit', 'Dataview bridge: non-string query rejected');
    check(calls.length === 7, 'Dataview bridge: invalid requests never reach plugin');
    console.log(`${count} Dataview bridge checks passed.`);
}
main().catch(error => { console.error(error); process.exitCode = 1; });
