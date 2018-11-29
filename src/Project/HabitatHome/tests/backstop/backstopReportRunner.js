var path = require('path');
var backstopDir = require('os').homedir() + '/AppData/Roaming/npm/node_modules/backstopjs';
var executeCommand = require(path.normalize(backstopDir + '/core/command/index'));
var makeConfig = require(path.normalize(backstopDir + '/core/util/makeConfig'));

module.exports = function (command, options) {
    var config = makeConfig(command, options);
    config.tempCompareConfigFileName = options.compareConfigFile;
    return executeCommand(command, config);
};