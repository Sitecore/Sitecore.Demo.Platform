"use strict";
var exec = require("child_process").exec;

module.exports = function (callback, options) {
    var syncScript = __dirname + "/SPE/./set-hostname.ps1" + " -instanceUrl " + options.siteHostName;
    var execOptions;
    var hostName = options.siteHostName;

    var process = exec("powershell -executionpolicy unrestricted \"" + syncScript + "\"", execOptions, function (err, stdout, stderr) {
        if (err !== null) throw err;
        console.log(stdout);
        callback();
    });

    
  process.stdout.on('data', function (data) {
    console.log(data.toString());
  });
  
  process.stderr.on('data', function (data) {
    console.log("Error: " + data.toString());
  });
  
  return process;
}