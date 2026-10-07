// Attach opaque markers during export, then remove them and bind exact output lines to source snapshots.
function stickyTaskTargets() {
    const prefix = 'STICKY' + require('crypto').randomUUID().replace(/-/g, '') + '_';
    const candidates = [];
    return {
        mark(text, candidate) {
            if (candidates.length >= 1000) return text;
            const id = candidates.push(candidate) - 1;
            return text + ' ' + prefix + id + 'END';
        },
        async finish(markdown) {
            const tasks = [], cache = new Map();
            const lines = markdown.split('\n');
            for (let outputLine = 0; outputLine < lines.length; outputLine++) {
                const marker = new RegExp(' ' + prefix + '(\\d+)END', 'g');
                const ids = [...lines[outputLine].matchAll(marker)].map(m => Number(m[1]));
                lines[outputLine] = lines[outputLine].replace(marker, '');
                if (ids.length !== 1 || !/^\s*- \[.\] /.test(lines[outputLine])) continue;
                const c = candidates[ids[0]];
                if (!c || typeof c.path !== 'string' || !Number.isInteger(c.line) || c.line < 0) continue;
                if (!cache.has(c.path)) {
                    const file = app.vault.getAbstractFileByPath(c.path);
                    let snapshot = null;
                    if (file?.extension === 'md' && file.stat.size <= 2000000) {
                        const bytes = await app.vault.adapter.readBinary(file.path);
                        if (bytes.byteLength <= 2000000) {
                            const text = new TextDecoder('utf-8', {fatal:true,ignoreBOM:true}).decode(bytes);
                            snapshot = {lines:text.replace(/^\uFEFF/, '').split(/\r?\n/),
                                hash:'cli:' + require('crypto').createHash('sha256').update(text, 'utf8').digest('hex')};
                        }
                    }
                    cache.set(c.path, snapshot);
                }
                const snapshot = cache.get(c.path);
                if (snapshot === null) continue;
                const source = snapshot.lines[c.line];
                const match = source?.match(/^(\s*(?:>\s*)*(?:[-*+]|\d+[.)])\s+\[)(.)(\]\s+)(.*)$/);
                if (!match || (c.original !== undefined ? source !== c.original :
                    match[2] !== c.status || match[4].trim() !== c.text?.split('\n')[0].trim())) continue;
                tasks.push({ outputLine, path: c.path, line: c.line, hash:snapshot.hash });
                // WPF has binary checkboxes; preserve custom source statuses until a real click.
                lines[outputLine] = lines[outputLine].replace(/^(\s*- \[).(\])/, '$1' + (match[2] === ' ' ? ' ' : 'x') + '$2');
            }
            return { markdown: lines.join('\n'), error: null, tasks };
        }
    };
}
