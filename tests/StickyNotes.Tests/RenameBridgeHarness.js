// Synthetic Obsidian API contract checks. Run: node tests/StickyNotes.Tests/RenameBridgeHarness.js
const fs = require('fs');
const vm = require('vm');
const assert = require('assert/strict');
const crypto = require('crypto');
const text = 'Note body';
const hash = 'cli:' + crypto.createHash('sha256').update(text).digest('hex');
const files = new Map();
const add = path => files.set(path, {path, extension: 'md', stat: {size: text.length}});
let renames = 0;
let failAfterMove = false;
const context = {
    require, TextDecoder, window: {},
    app: {
        vault: {
            adapter: {getBasePath: () => 'C:/fixture', readBinary: async () => Buffer.from(text)},
            getAbstractFileByPathInsensitive: path => files.get(path),
            rename: () => { throw Error('Must use fileManager for link updates'); }
        },
        fileManager: {renameFile: async (file, target) => {
            assert(!files.has(target));
            renames++; files.delete(file.path); file.path = target; files.set(target, file);
            if (failAfterMove) throw Error('Link update failed after move');
        }}
    }
};
vm.createContext(context);
vm.runInContext(fs.readFileSync(require('path').join(__dirname, '../../src/StickyNotes/FoldersBridge.js'), 'utf8'), context);
const request = (path, target) => ({mode: 'rename', root: 'C:/fixture', path, target, hash, operation: crypto.randomBytes(16).toString('hex')});
(async () => {
    add('old.md');
    const first = request('old.md', 'new.md');
    await Promise.all([context.stickyTasksPreview(first), context.stickyTasksPreview(first)]);
    assert.equal(renames, 1); assert(files.has('new.md')); assert(!files.has('old.md'));
    await assert.rejects(context.stickyTasksPreview({...first, target: 'different.md'}));
    add('collision.md');
    await assert.rejects(context.stickyTasksPreview(request('new.md', 'collision.md')));
    await assert.rejects(context.stickyTasksPreview({...request('new.md', 'other.md'), hash: 'stale'}));
    await assert.rejects(context.stickyTasksPreview(request('new.md', '../escape.md')));
    await assert.rejects(context.stickyTasksPreview(request('new.md', 'sub/moved.md')));
    await assert.rejects(context.stickyTasksPreview({...request('new.md', 'other.md'), root: 'C:/other'}));
    assert.equal(renames, 1);
    failAfterMove = true;
    const partial = request('new.md', 'partial.md');
    await assert.rejects(context.stickyTasksPreview(partial));
    await assert.rejects(context.stickyTasksPreview(partial));
    assert.equal(renames, 2); assert(files.has('partial.md'));
    console.log('PASS: native rename, serialized retry, operation identity, collision, stale hash, traversal, same-directory boundary, vault boundary, and partial-failure retry');
})().catch(error => { console.error(error); process.exitCode = 1; });
