#!/usr/bin/env node

var path = require('path');
var backstopReportRunner = require('./backstopReportRunner');

var options = {
    compareConfigFile: path.normalize(__dirname + '/backstopReportConfig.json'),
    config: 'backstop.json'
};
var exitCode = 0;
backstopReportRunner('_report', options).catch(function () {
    exitCode = 1;
});

// Wait for the stdout buffer to drain.
process.on('exit', function () {
    process.exit(exitCode);
});
