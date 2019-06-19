import sendFile from './fileActionResolver';
import path from 'path';

var getLogMessage = function(files, stylePath) {
    var message = 'Action: ';
    if (typeof files.origFile !== 'undefined') {
        message += files.origFile.event;
    } else if (typeof files.file !== 'undefined' && files.file.event) {
        message += files.file.event;
    }
    message += '. File: ';
    message += stylePath;
    message += '/';
    message += path.basename(files.file.path);
    return message;
}

export default function(files, style = 'styles') {
    console.log(getLogMessage(files, style).yellow);
    return style;
};