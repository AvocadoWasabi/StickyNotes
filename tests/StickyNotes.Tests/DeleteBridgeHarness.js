// Run: node tests/StickyNotes.Tests/DeleteBridgeHarness.js
const fs = require('fs'), path = require('path'), os = require('os');
const vm = require('vm'), assert = require('assert/strict'), crypto = require('crypto');
const root = fs.mkdtempSync(path.join(os.tmpdir(), 'StickyNotes-delete-'));
const text = '\uFEFF---\r\ntitle: 日本語\r\n---\r\nBody\r\n';
const hash = 'cli:' + crypto.createHash('sha256').update(text).digest('hex');
const files = new Map(); let trashes = 0, failAfterTrash = false, changeOnRead = false, reads = 0;
function add(name) {
    fs.writeFileSync(path.join(root, name), text);
    files.set(name, {path:name, extension:'md', stat:{size:Buffer.byteLength(text)}});
}
const context = {require, TextDecoder, window:{}, app:{vault:{
    adapter:{getBasePath:() => root, readBinary:async name => {
        if (changeOnRead && ++reads === 2) fs.appendFileSync(path.join(root, name), 'new edit');
        return fs.readFileSync(path.join(root, name));
    }},
    getAbstractFileByPathInsensitive:name => files.get(name),
    trash:async (file, system) => {
        assert.equal(system, false); trashes++;
        fs.renameSync(path.join(root, file.path), path.join(root, 'trashed-' + trashes + '.md'));
        files.delete(file.path);
        if (failAfterTrash) throw Error('Lost trash result');
    }
}}};
vm.createContext(context);
vm.runInContext(fs.readFileSync(path.join(__dirname, '../../src/StickyNotes/FoldersBridge.js'), 'utf8'), context);
function request(name) {
    const operation = crypto.randomBytes(16).toString('hex');
    const backup = path.join(root, operation); fs.mkdirSync(backup);
    return {mode:'delete', root, path:name, hash, backup, operation};
}
(async () => {
    add('note.md'); const first = request('note.md');
    await Promise.all([context.stickyTasksPreview(first), context.stickyTasksPreview(first)]);
    assert.equal(trashes, 1); assert(!files.has('note.md'));
    assert.equal(fs.readFileSync(path.join(first.backup, 'note.md'), 'utf8'), text);
    add('note.md'); await context.stickyTasksPreview(first);
    assert(files.has('note.md')); assert.equal(trashes, 1); // Same-name replacement survives retries.
    await assert.rejects(context.stickyTasksPreview({...first, path:'other.md'}));
    await assert.rejects(context.stickyTasksPreview({...request('note.md'), hash:'cli:'+'a'.repeat(64)}));
    await assert.rejects(context.stickyTasksPreview({...request('note.md'), root:root+'-other'}));
    await assert.rejects(context.stickyTasksPreview(request('../outside.md')));
    const denied = request('note.md'); fs.rmdirSync(denied.backup); fs.writeFileSync(denied.backup, 'blocked');
    await assert.rejects(context.stickyTasksPreview(denied)); assert(files.has('note.md')); assert.equal(trashes, 1);
    changeOnRead = true; reads = 0;
    await assert.rejects(context.stickyTasksPreview(request('note.md')));
    assert(fs.readFileSync(path.join(root, 'note.md'), 'utf8').endsWith('new edit')); assert.equal(trashes, 1);
    changeOnRead = false; add('note.md'); failAfterTrash = true;
    const partial = request('note.md');
    await assert.rejects(context.stickyTasksPreview(partial));
    add('note.md'); await assert.rejects(context.stickyTasksPreview(partial));
    assert.equal(trashes, 2); assert(files.has('note.md'));
    assert.equal(fs.readFileSync(path.join(partial.backup, 'note.md'), 'utf8'), text);
    console.log('PASS: deletion backup/BOM, vault trash, serialized retry, replacement protection, identity, hash conflict, boundaries, backup failure, concurrent edit and partial failure');
})().catch(error => { console.error(error); process.exitCode = 1; }).finally(() => fs.rmSync(root, {recursive:true, force:true}));
