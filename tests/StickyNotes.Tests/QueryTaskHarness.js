const fs = require('node:fs');
const path = require('node:path');
const assert = require('node:assert/strict');
const crypto = require('node:crypto');

async function main() {
    let input = ''; for await (const chunk of process.stdin) input += chunk;
    const q = JSON.parse(input.replace(/^\uFEFF/, ''));
    const filePath = path.join(q.root, 'target.md');
    let count = 0, reads = 0, unloaded = 0;
    const check = (condition, label) => { assert.ok(condition, label); console.log('PASS: ' + label); count++; };
    const reset = () => fs.writeFileSync(filePath, q.text, 'utf8');
    const hash = text => 'cli:' + crypto.createHash('sha256').update(text, 'utf8').digest('hex');
    class ListItem {
        constructor(line, isTask = false) {
            this.taskLocation = {path:'target.md',lineNumber:line};
            this.originalMarkdown = q.text.replace(/^\uFEFF/, '').split('\r\n')[line];
            this.description = this.originalMarkdown.replace(/^\s*- \[.\] /, '');
            this.isTask = isTask; this.status = {symbol:' '}; this.statusCharacter = ' ';
            this.parent = null; this.children = [];
        }
        findClosestParentTask() { let p = this.parent; while (p && !(p instanceof Task)) p = p.parent; return p; }
    }
    class Task extends ListItem {
        constructor(line) { super(line, true); }
        toString() { return this.description; }
    }
    const task = line => Object.assign(new Task(line), {
        toggleWithRecurrenceInUsersOrder: () => [{toFileLineString:() => '- [x] Recurring ✅ 2026-10-08'}, {toFileLineString:() => '- [ ] Recurring 📅 2026-10-09'}]});
    const tasks = [task(4),task(3),task(5)];
    let tree = false, exportFailure = false, searchFailure = false, missingApi = false;
    class MarkdownRenderer822 {
        async renderQuery(result) {
            if (exportFailure) throw new Error('Synthetic export failure');
            const lines = [];
            for (const group of result.taskGroups.groups) {
                lines.push('# Group');
                const added = new Set();
                const render = (items, depth = 0) => {
                    for (const item of items) {
                        if (tree && (added.has(item) || items.includes(item.findClosestParentTask()) && !added.has(item.findClosestParentTask()))) continue;
                        if (item instanceof Task) lines.push('    '.repeat(depth) + '- [' + item.status.symbol + '] ' + item.toString());
                        else if (tree) lines.push('    '.repeat(depth) + '- [' + item.statusCharacter + '] ' + item.description);
                        if (tree) { render(item.children, depth + 1); added.add(item); }
                    }
                };
                render(group.tasks);
            }
            this.markdown = lines.join('\n');
        }
    }
    const plugin = {manifest:{version:'8.2.2'},getState:()=>'Warm',getTasks:()=>tasks,
        queryRenderer:{addQueryRenderChild:async (_, __, context) => {
            context.addChild({load:()=>{},unload:()=>unloaded++,queryResultsRenderer:{query:{},
                performSearch() { this.filteredQueryResult = missingApi ? {} : {searchErrorMessage:searchFailure ? 'Synthetic search failure' : undefined,
                    taskGroups:{groups:[{tasks:[...tasks]},{tasks:[tasks[0]]}]}}; },
                async resultsAsMarkdown() {
                    assert.equal(this.markdownRenderer, undefined);
                    const exporter = new MarkdownRenderer822();
                    await exporter.renderQuery(this.filteredQueryResult);
                    return exporter.markdown;
                }}});
        }}};
    const dvTask = line => ({task:true,path:'target.md',line,status:' ',text:q.text.split('\r\n')[line].replace(/^- \[ \] /,''),children:[]});
    const dv = {manifest:{version:'mock'},index:{initialized:true},api:{
        query:async () => ({successful:true,value:{type:'task',values:[{key:'Grouped',rows:[{...dvTask(3),children:[{task:true,path:'target.md',line:6,status:' ',text:'Child',children:[]}]},dvTask(4)]}]}}),
        markdownTaskList:items => {
            const render = (rows, depth=0) => rows.map(item => item.rows ? '# Grouped\n' + render(item.rows,depth) :
                ' '.repeat(depth) + '- [' + item.status + '] ' + (item.visual ?? item.text) + '\n' + render(item.children,depth+1)).join('');
            return render(items);
        }
    }};
    global.document = {createElement:()=>({})};
    global.window = {};
    global.app = {plugins:{plugins:{'obsidian-tasks-plugin':plugin,dataview:dv}},vault:{
        adapter:{getBasePath:()=>q.root, readBinary:async p => { reads++; return fs.readFileSync(path.join(q.root,p)); }},
        getAbstractFileByPath:p => ['target.md','query.md'].includes(p) ? {path:p,extension:'md',stat:{size:fs.statSync(path.join(q.root,p)).size}} : null,
        process:async (file, callback) => { const full = path.join(q.root,file.path); const updated = callback(fs.readFileSync(full,'utf8')); fs.writeFileSync(full,updated,'utf8'); return updated; }
    }};
    app.vault.getRoot = () => ({path:'/',children:[]});
    app.vault.getMarkdownFiles = () => [{path:'target.md'},{path:'query.md'},{path:'nested/a.md'},{path:'.hidden/secret.md'}];
    const run = async code => JSON.parse((await eval(code)).replace(/^STICKY_TASKS_PREVIEW:/,''));
    reset(); fs.writeFileSync(path.join(q.root,'query.md'),'```tasks\nnot done\n```');
    const listed = await run(q.folderCode);
    check(listed.notes.join(',') === 'nested/a.md,query.md,target.md', 'Folder bridge lists nested Markdown and skips hidden folders without reading note content');
    let result = await run(q.tasksCode);
    check(result.results[0].tasks.map(t=>t.line).join(',') === '4,3,5,4', 'Tasks mapping follows sorted/grouped exports including duplicate descriptions and repeated groups');
    check(result.results[0].tasks.every(t=>t.path==='target.md' && t.hash===hash(q.text)) && !result.results[0].markdown.includes('STICKY'), 'Tasks mapping includes source hashes and removes opaque markers');
    check(reads===1 && unloaded===1, 'Tasks reads each source once and unloads detached renderer');
    const childTask = task(6), childItem = new ListItem(6);
    tasks[2].children = [childTask]; childTask.parent = tasks[2]; tasks.unshift(childTask); tree = true;
    result = await run(q.tasksCode);
    check(result.results[0].tasks.map(t=>t.line).join(',') === '4,3,5,6,6', 'Tasks 8.2.2 preserves parent identity and renders child once per group');
    check(childTask.parent === tasks[3] && !childTask.toString().includes('STICKY') && !Object.hasOwn(childTask, 'toString'), 'Export leaves shared task instances and prototypes unchanged');
    tasks.shift(); tasks[2].children = [childItem]; childItem.parent = tasks[2];
    result = await run(q.tasksCode);
    check(result.results[0].tasks.map(t=>t.line).join(',') === '4,3,5,6,4', 'Nested non-Task checkbox list items retain verified source mapping');
    tasks[2].children = []; tree = false;
    exportFailure = true; const unloadBefore = unloaded; result = await run(q.tasksCode); exportFailure = false;
    check(result.results[0].error === 'Synthetic export failure' && unloaded === unloadBefore + 1 && !tasks[0].toString().includes('STICKY'), 'Export failure unloads renderer and leaves cache untouched');
    searchFailure = true; result = await run(q.tasksCode); searchFailure = false;
    check(result.results[0].error === 'Synthetic search failure', 'Search errors remain errors rather than successful Markdown');
    missingApi = true; result = await run(q.tasksCode); missingApi = false;
    check(result.results[0].error === 'STICKY_TASKS_UNSUPPORTED_API', 'Unsupported future result shape fails safely');
    for (const version of ['7.23.1','8.0.0','8.2.1','8.2.2-beta.1','unknown']) {
        plugin.manifest.version = version;
        const before = unloaded, backupsBefore = fs.readdirSync(q.root).join(',');
        result = await run(q.tasksCode);
        check(result.error === 'STICKY_TASKS_UPDATE_REQUIRED:' + version && unloaded === before, 'Unsupported version ' + version + ' rejected before renderer creation');
        result = await run(q.tasksToggle);
        check(result.error === 'STICKY_TASKS_UPDATE_REQUIRED:' + version && fs.readFileSync(filePath,'utf8') === q.text && fs.readdirSync(q.root).join(',') === backupsBefore, 'Old Tasks query click is rejected without write or backup');
        result = await run(q.transform);
        check(result.error === 'STICKY_TASKS_UPDATE_REQUIRED:' + version, 'Old Tasks ordinary click requests update');
    }
    for (const version of ['8.2.2','8.2.2+build.1','8.2.10','8.10.0','9.0.0']) {
        plugin.manifest.version = version; result = await run(q.tasksCode);
        check(!result.error && !result.results[0].error, 'Numeric version comparison accepts ' + version + ' with supported API');
    }
    plugin.manifest.version = '8.2.2';
    result = await run(q.dataviewCode);
    check(result.results[0].tasks.map(t=>t.line).join(',') === '3,6,4', 'Dataview grouped/nested tasks retain exact source positions');
    check(result.results[0].tasks.map(t=>t.outputLine).join(',') === '1,2,3', 'Dataview export output lines map without guessing by description');
    check(fs.readFileSync(filePath,'utf8')===q.text, 'Querying and mapping never writes source notes');
    const beforeQuery = fs.readFileSync(path.join(q.root,'query.md'),'utf8');
    result = await run(q.dataviewToggle);
    check(result.text === q.text.replace('- [ ] Same\r\n- [ ] Recurring', '- [x] Same\r\n- [ ] Recurring'), 'Dataview click changes only selected duplicate task and preserves BOM/CRLF/YAML');
    check(fs.readFileSync(path.join(q.root,'query.md'),'utf8')===beforeQuery, 'Checkbox updates target note, never query Markdown');
    let backups = fs.readdirSync(q.backup);
    check(backups.length===1 && fs.readFileSync(path.join(q.backup,backups[0]),'utf8')===q.text, 'Checkbox write makes exact recoverable backup');
    result = await run(q.dataviewToggle);
    check(result.error==='STICKY_CONFLICT' && fs.readdirSync(q.backup).length===1, 'Repeated/stale click rejected without another write or backup');
    reset(); result = await run(q.tasksToggle);
    check(result.text.includes('- [x] Recurring ✅ 2026-10-08\r\n- [ ] Recurring 📅 2026-10-09\r\n  - [ ] Child\r\nSuffix'), 'Tasks click uses native status/recurrence output while preserving child and surrounding lines');
    reset(); fs.appendFileSync(filePath,'external'); const conflictBefore = fs.readFileSync(filePath,'utf8');
    result = await run(q.tasksToggle);
    check(result.error==='STICKY_CONFLICT' && fs.readFileSync(filePath,'utf8')===conflictBefore, 'Whole-note conflict check preserves external edits');
    reset(); fs.writeFileSync(q.denied,'not a folder'); result = await run(q.deniedToggle);
    check(!!result.error && fs.readFileSync(filePath,'utf8')===q.text, 'Backup failure prevents checkbox write');
    reset(); tasks[0].originalMarkdown='- [ ] stale cache'; result = await run(q.tasksCode);
    check(result.results[0].tasks.every(t=>t.line!==4), 'Stale plugin index disables unsafe source mapping');
    result = await run(q.invalidToggle);
    check(!!result.error && fs.readFileSync(filePath,'utf8')===q.text, 'Invalid provider never writes');
    let nativeCalls = 0;
    result = await run(q.availabilityCode); check(result.available === true, 'Automatic completion detects loaded Tasks without a source note');
    plugin.apiV1 = {executeToggleTaskDoneCommand: (line, source) => {
        nativeCalls++; assert.equal(source, 'target.md');
        assert.equal(line, '- [ ] Recurring `literal` ${notCode} 日本語');
        return '- [x] Completed ✅ 2026-10-08\n- [ ] Next 📅 2026-10-09';
    }};
    result = await run(q.transform);
    check(nativeCalls === 1 && result.text.includes('\n- [ ] Next'), 'Ordinary checkbox delegates to public Tasks API and accepts recurrence output');
    check(fs.readFileSync(filePath,'utf8') === q.text, 'Transform does not write; existing save pipeline retains conflict and backup authority');
    result = await run(q.transformNoop);
    check(nativeCalls === 1 && result.text === '- [x] Done', 'Redundant checkbox operation does not cycle native status');
    result = await run(q.transformInvalid);
    check(!!result.error && nativeCalls === 1, 'Multiline input rejected before calling native API');
    result = await run(q.transformWrongVault);
    check(!!result.error && nativeCalls === 1, 'Wrong vault rejected before native task transformation');
    plugin.apiV1.executeToggleTaskDoneCommand = () => '';
    result = await run(q.transform); check(result.text === '', 'Native on-completion deletion is a valid transformation');
    plugin.apiV1.executeToggleTaskDoneCommand = () => { throw new Error('Synthetic Tasks failure'); };
    result = await run(q.transform); check(result.error === 'Synthetic Tasks failure', 'Plugin error is surfaced without plain-checkbox fallback');
    delete plugin.apiV1;
    result = await run(q.transform); check(!!result.error, 'Installed plugin with unsupported API fails safely');
    delete app.plugins.plugins['obsidian-tasks-plugin'];
    result = await run(q.availabilityCode); check(result.available === false, 'Missing/disabled Tasks leaves automatic completion off');
    result = await run(q.transformDone); check(result.text === '- [ ] Done', 'Missing Tasks plugin keeps ordinary Markdown checkbox support');
    console.log(`${count} query task checks passed.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
