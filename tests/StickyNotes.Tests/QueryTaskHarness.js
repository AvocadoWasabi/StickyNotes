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
    const task = line => ({taskLocation:{path:'target.md',lineNumber:line}, originalMarkdown:q.text.replace(/^\uFEFF/, '').split('\r\n')[line],
        toggleWithRecurrenceInUsersOrder: () => [{toFileLineString:() => '- [x] Recurring ✅ 2026-10-08'}, {toFileLineString:() => '- [ ] Recurring 📅 2026-10-09'}]});
    const tasks = [task(4),task(3),task(5)];
    const plugin = {manifest:{version:'mock'},getState:()=>'Warm',getTasks:()=>tasks,
        queryRenderer:{addQueryRenderChild:async (_, __, context) => {
            const exporter = {formatTask:t => t.originalMarkdown, formatListItem:t => t.originalMarkdown};
            context.addChild({load:()=>{},unload:()=>unloaded++,queryResultsRenderer:{query:{},markdownRenderer:exporter,
                performSearch:()=>{},resultsAsMarkdown:async () => '# Group\n' + tasks.map(t => exporter.formatTask(t)).join('\n') + '\n# Again\n' + exporter.formatTask(tasks[0])}});
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
    console.log(`${count} query task checks passed.`);
}
main().catch(error=>{console.error(error);process.exitCode=1;});
