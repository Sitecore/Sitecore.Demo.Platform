var fs = require('fs');

var configUpdated = require('./setThemePath');

var renameGulpBabel = function() {
    if (fs.existsSync('gulpfilebabel.js')) {
        fs.rename('gulpfilebabel.js', 'gulpfile.babel.js', function(err) {
            if (err) console.log('ERROR: ' + err);
        });
    }
}

renameGulpBabel();
//Updating path at serverConfig
configUpdated.updateConfig();