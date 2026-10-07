// Only used by the optional synthetic transport smoke test. Never accesses a real vault.
const fs = require('fs');
const path = require('path');
const input = JSON.parse(fs.readFileSync(0, 'utf8'));
const resolve = name => {
    const full = path.resolve(input.root, name);
    if (!full.startsWith(path.resolve(input.root) + path.sep)) throw Error('Outside test root');
    return full;
};
global.app = { vault: {
    adapter: {
        getBasePath: () => input.root,
        readBinary: async name => new Uint8Array(fs.readFileSync(resolve(name))).buffer
    },
    getAbstractFileByPath: name => fs.existsSync(resolve(name)) ?
        { path: name, extension: path.extname(name).slice(1), stat: fs.statSync(resolve(name)) } : null,
    process: async (file, update) => {
        const updated = update(fs.readFileSync(resolve(file.path), 'utf8'));
        fs.writeFileSync(resolve(file.path), updated, 'utf8');
        return updated;
    }
} };
Promise.resolve(eval(input.code)).then(result => process.stdout.write(result + '\n'))
    .catch(error => { process.stderr.write(String(error)); process.exitCode = 1; });
