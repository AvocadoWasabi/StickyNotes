async function stickyFolderOperation(q) {
    const fail = text => { throw Error(text); };
    const normalize = text => text.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(q.root)) fail('Wrong vault.');
    const clean = text => {
        if (text === '.') return '';
        if (typeof text !== 'string' || !text || text.split('/').some(p => !p || p.startsWith('.') || /[\\:*?"<>|\x00-\x1f]/.test(p))) fail('Invalid folder path.');
        return text;
    };
    const get = path => path ? app.vault.getAbstractFileByPathInsensitive(path) : app.vault.getRoot();
    const folder = file => file && Array.isArray(file.children);
    const hash = text => 'cli:' + require('crypto').createHash('sha256').update(text, 'utf8').digest('hex');
    const read = async file => {
        if (file.stat.size > 2000000) fail('Source note exceeds 2 MB.');
        return new TextDecoder('utf-8', {fatal:true,ignoreBOM:true}).decode(await app.vault.adapter.readBinary(file.path));
    };
    const ensure = async path => {
        let current = '';
        for (const part of path.split('/').filter(Boolean)) {
            current = current ? current + '/' + part : part;
            if (!get(current)) await app.vault.createFolder(current);
            if (!folder(get(current))) fail('Folder path collides with a file.');
        }
    };
    if (q.mode === 'folders') {
        const folders = app.vault.getAllLoadedFiles().filter(folder).map(f => f.path).filter(p => p && p !== '/');
        if (folders.length > 10000) fail('Too many folders.');
        const parent = app.fileManager.getNewFileParent('').path;
        return { folders: ['.', ...folders], defaultFolder: parent === '/' || !parent ? '.' : parent };
    }
    const path = clean(q.path);
    if (q.mode === 'ensure') { await ensure(path); return {}; }
    if (q.mode === 'exists') return { exists: !!get(path) };
    if (q.mode === 'browse') {
        if (!folder(get(path))) fail('The selected folder is missing.');
        const notes = app.vault.getMarkdownFiles().filter(f => (!path || f.path.toLowerCase().startsWith(path.toLowerCase() + '/')) &&
            !f.path.split('/').some(p => p.startsWith('.'))).map(f => f.path).sort();
        if (notes.length > 1000) fail('Too many notes (1000). Choose a smaller folder.');
        return {notes};
    }
    if (q.mode === 'sticky') {
        const excluded = clean(q.exclude);
        const under = (p, base) => !base || p.toLowerCase().startsWith(base.toLowerCase() + '/');
        const notes = [];
        for (const file of app.vault.getMarkdownFiles()) {
            if (!under(file.path, path) || (excluded && under(excluded, path) && under(file.path, excluded))) continue;
            const cached = app.metadataCache.getFileCache(file)?.frontmatter;
            if (cached?.type !== 'sticky' || !cached.id) continue;
            const text = await read(file);
            const match = text.replace(/^\uFEFF/, '').match(/^---\r?\n([\s\S]*?)\r?\n---(?:\r?\n|$)/);
            const meta = match && require('obsidian').parseYaml(match[1]);
            if (meta?.type !== 'sticky' || !/^[0-9a-f]{8}(-[0-9a-f]{4}){3}-[0-9a-f]{12}$/i.test(String(meta.id))) continue;
            notes.push({path:file.path,hash:hash(text)});
            if (notes.length > 1000) fail('Too many sticky notes (1000).');
        }
        return {notes};
    }
    if (q.mode === 'move') {
        const target = clean(q.target);
        const file = get(path), existing = get(target);
        if (!file && existing?.extension === 'md' && hash(await read(existing)) === q.hash) return {};
        if (!file || file.extension !== 'md' || existing || hash(await read(file)) !== q.hash) fail('Move conflict; no overwrite.');
        await ensure(target.split('/').slice(0, -1).join('/'));
        await app.vault.rename(file, target);
        return {};
    }
    fail('Unknown folder operation.');
}

// CLI timeouts do not cancel JavaScript. Rollback must wait for an earlier move.
async function stickyTasksPreview(q) {
    const prior = window.__stickyNotesFolderQueue || Promise.resolve();
    const result = prior.catch(() => {}).then(() => stickyFolderOperation(q));
    window.__stickyNotesFolderQueue = result.catch(() => {});
    return await result;
}
