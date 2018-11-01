#!/usr/bin/env node

const fs = require('fs');
var path = require('path');
var backstopReportRunner = require('./backstopReportRunner');



function main() {
    var options = {
        compareConfigFile: writeConfig(),
        config: 'backstop.json'
    };
    console.log(JSON.stringify(options));
    var exitCode = 0;
    backstopReportRunner('_report', options).catch(function () {
        exitCode = 1;
    });
    
    // Wait for the stdout buffer to drain.
    process.on('exit', function () {
        process.exit(exitCode);
    });
}



function writeConfig() {
    // Find latest subdirectory
    var latest;
    fs.readdirSync(path.normalize('./backstop_data/bitmaps_test')).forEach(file => {
        if (!latest || file > latest)
            latest = file;
    });

    var config = {
        compareConfig: {
            testPairs: []
        }
    };

    var fullBackstopDataPath = path.normalize(__dirname + '/backstop_data');
    fs.readdirSync(path.normalize('./backstop_data/bitmaps_test/' + latest)).forEach(file => {
        if (file.startsWith('failed_diff_'))
            return;
            
        var pair = {
            reference: path.normalize(fullBackstopDataPath + '/bitmaps_reference/' + file),
            test: path.normalize(fullBackstopDataPath + '/bitmaps_test/' + latest + '/' + file),
            selector: "document",
            fileName: file,
            label: file,
            requireSameDimensions: true,
            misMatchThreshold: 0.1,
            url: '',
            referenceUrl: ''
        };
        config.compareConfig.testPairs.push(pair);
    });


    fs.writeFileSync('./backstopReportConfig.json', JSON.stringify(config, null, 2), 'utf-8');

    return path.normalize(__dirname + '/backstopReportConfig.json');
}



main();
