var path = require("path");

/**
 * Remove byte order marker. This catches EF BB BF (the UTF-8 BOM)
 * because the buffer-to-string conversion in `fs.readFileSync()`
 * translates it to FEFF, the UTF-16 BOM.
 */
function stripBOM(content) {
  if (content.charCodeAt(0) === 0xFEFF) {
    content = content.slice(1);
  }
  return content;
}


function requireFromString(code, filename) {
    try {
        var Module = module.constructor;
        var m = new Module(filename, module.parent);
        m.filename = filename;
        m.paths = Module._nodeModulePaths(path.dirname(filename));
        code = stripBOM(code);
        var extension = path.extname(filename) || '.js';
        if (extension=='.js') {
            m._compile(code, filename);
        } else {
            m.exports = JSON.parse(code);
        }
        return m.exports;
    } catch (err) {
        err.message = filename + ': ' + err.message;
        throw err;
    }
}

module.exports = requireFromString;
