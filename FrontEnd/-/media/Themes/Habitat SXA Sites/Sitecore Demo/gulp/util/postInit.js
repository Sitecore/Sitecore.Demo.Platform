var fs = require("fs");

var configUpdated = require("./setThemePath");

var copyGulpBabel = function() {
    if (fs.existsSync("gulpfilebabel.js")) {
        fs.copyFile("gulpfilebabel.js", "gulpfile.babel.js", err => {
            if (err) throw err;
            console.log("gulpfilebabel.js was copied to gulpfile.babel.js");
        });
    }
};

var restoreSourcesFolder = function() {
    if (!fs.existsSync("sources")) {
        try {
            fs.mkdirSync("sources");
        } catch (err) {
            if (err.code !== "EEXIST") console.log("ERROR: " + err);
        }
    }
};

copyGulpBabel();
restoreSourcesFolder();
//Updating path at serverConfig
configUpdated.updateConfig();
