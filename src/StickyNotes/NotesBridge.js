// Executed inside the existing Obsidian instance by the official CLI.
// Input is data, never interpolated JavaScript. No clipboard access.
async function stickyTasksPreview(request) {
    const fail = message => { throw new Error(message); };
    const normalize = value => value.replace(/\\/g, '/').replace(/\/+$/, '').toLowerCase();
    if (normalize(app.vault.adapter.getBasePath()) !== normalize(request.root))
        fail('The selected Obsidian vault does not match the configured vault folder.');
    const fingerprint = text => 'cli:' + require('crypto').createHash('sha256').update(text, 'utf8').digest('hex');
    const snapshot = text => ({ text, hash: fingerprint(text) });
    const checkSize = text => {
        if (Buffer.byteLength(text, 'utf8') > 2000000) fail('Source note exceeds the preview size limit (2 MB).');
        return text;
    };
    if (request.mode === 'create') {
        checkSize(request.text);
        if (request.name) {
            if (/[\\/]/.test(request.name) || !request.name.endsWith('.md')) fail('Invalid note name.');
            const parent = request.folder == null ? app.fileManager.getNewFileParent('') :
                (request.folder === '.' ? app.vault.getRoot() : app.vault.getAbstractFileByPathInsensitive(request.folder));
            if (!parent || !Array.isArray(parent.children)) fail('The sticky-note folder is missing. Select it again in Settings.');
            const prefix = parent.path.replace(/^\/+|\/+$/g, '');
            request.path = (prefix ? prefix + '/' : '') + request.name;
        }
        if (app.vault.getAbstractFileByPath(request.path)) fail('The new note already exists.');
        await app.vault.create(request.path, request.text);
        return { ...snapshot(request.text), path: request.path };
    }
    const file = app.vault.getAbstractFileByPath(request.path);
    if (!file || file.extension !== 'md') fail('The source Markdown note is not in this vault.');
    if (file.stat.size > 2000000) fail('Source note exceeds the preview size limit (2 MB).');
    if (request.mode === 'read') {
        // Keep the BOM for Vault.process hashing; reject invalid UTF-8 like the local reader.
        const bytes = await app.vault.adapter.readBinary(file.path);
        return snapshot(new TextDecoder('utf-8', { fatal: true, ignoreBOM: true }).decode(bytes));
    }
    if (request.mode === 'save' || request.mode === 'toggle-query-task') {
        const text = await app.vault.process(file, current => {
            if (fingerprint(current) !== request.hash) fail('STICKY_CONFLICT');
            let updated;
            if (request.mode === 'toggle-query-task') {
                if (!Number.isInteger(request.line) || request.line < 0 || typeof request.checked !== 'boolean' ||
                    !['tasks', 'dataview'].includes(request.provider)) fail('Invalid task update.');
                const parts = current.split(/(\r?\n)/), offset = request.line * 2;
                const original = parts[offset]?.replace(/^\uFEFF/, '');
                const match = original?.match(/^(\s*(?:>\s*)*(?:[-*+]|\d+[.)])\s+\[)(.)(\]\s+.*)$/);
                if (!match || (match[2] !== ' ') === request.checked) fail('STICKY_CONFLICT');
                let replacement = match[1] + (request.checked ? 'x' : ' ') + match[3];
                if (request.provider === 'tasks') {
                    const plugin = app.plugins.plugins['obsidian-tasks-plugin'];
                    if (plugin?.getState?.() !== 'Warm') fail('Tasks is not ready.');
                    const matches = plugin.getTasks().filter(t => t.taskLocation?.path === file.path && t.taskLocation.lineNumber === request.line);
                    if (matches.length === 1) {
                        const task = matches[0];
                        if (task.originalMarkdown !== original || typeof task.toggleWithRecurrenceInUsersOrder !== 'function') fail('STICKY_CONFLICT');
                        const toggled = task.toggleWithRecurrenceInUsersOrder();
                        if (!Array.isArray(toggled) || toggled.length === 0 || toggled.length > 100) fail('Invalid Tasks update.');
                        replacement = toggled.map(t => t.toFileLineString()).join(current.includes('\r\n') ? '\r\n' : '\n');
                    } else if (matches.length > 1) fail('STICKY_CONFLICT');
                }
                parts[offset] = (offset === 0 && current.startsWith('\uFEFF') ? '\uFEFF' : '') + replacement;
                updated = checkSize(parts.join(''));
            } else updated = checkSize((current.startsWith('\uFEFF') ? '\uFEFF' : '') + request.text.replace(/^\uFEFF+/, ''));
            // Back up the exact string provided by Obsidian before applying the edit.
            const fs = require('fs');
            const path = require('path');
            fs.mkdirSync(request.backup, { recursive: true });
            fs.writeFileSync(path.join(request.backup, Date.now() + '-' + require('crypto').randomUUID() + '.md'), current, { encoding: 'utf8', flag: 'wx' });
            return updated;
        });
        return snapshot(text);
    }
    fail('Unknown note operation.');
}
