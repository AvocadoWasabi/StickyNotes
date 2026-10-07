// Executed inside the existing Obsidian instance by the official CLI.
// Input is data, never interpolated JavaScript. No clipboard access.
async function stickyTasksPreview(request) {
    const fail = message => { throw new Error(message); };
    const normalize = value => value.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(request.root))
        fail('The selected Obsidian vault does not match the configured vault folder.');
    if (request.mode === 'list') {
        const paths = app.vault.getMarkdownFiles().map(file => file.path);
        if (paths.length > 10000) fail('Vault file limit exceeded (10,000).');
        return { paths };
    }
    const fingerprint = text => 'cli:' + require('crypto').createHash('sha256').update(text, 'utf8').digest('hex');
    const snapshot = text => ({ text, hash: fingerprint(text) });
    if (request.mode === 'create') {
        if (app.vault.getAbstractFileByPath(request.path)) fail('The new note already exists.');
        await app.vault.create(request.path, request.text);
        return snapshot(request.text);
    }
    const file = app.vault.getAbstractFileByPath(request.path);
    if (!file || file.extension !== 'md') fail('The source Markdown note is not in this vault.');
    if (file.stat.size > 2000000) fail('Source note exceeds the preview size limit (2 MB).');
    if (request.mode === 'read') {
        // Keep the BOM for Vault.process hashing; reject invalid UTF-8 like the local reader.
        const bytes = await app.vault.adapter.readBinary(file.path);
        return snapshot(new TextDecoder('utf-8', { fatal: true, ignoreBOM: true }).decode(bytes));
    }
    if (request.mode === 'save') {
        const text = await app.vault.process(file, current => {
            if (fingerprint(current) !== request.hash) fail('STICKY_CONFLICT');
            // Back up the exact string provided by Obsidian before applying the edit.
            const fs = require('fs');
            const path = require('path');
            fs.mkdirSync(request.backup, { recursive: true });
            fs.writeFileSync(path.join(request.backup, Date.now() + '-' + require('crypto').randomUUID() + '.md'), current, { encoding: 'utf8', flag: 'wx' });
            return (current.startsWith('\uFEFF') ? '\uFEFF' : '') + request.text.replace(/^\uFEFF+/, '');
        });
        return snapshot(text);
    }
    fail('Unknown note operation.');
}
