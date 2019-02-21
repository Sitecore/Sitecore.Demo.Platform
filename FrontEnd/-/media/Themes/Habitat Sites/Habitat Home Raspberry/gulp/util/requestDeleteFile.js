import config from '../config';
import upload from 'gulp-upload';
import path from 'path';
import gulp from 'gulp';
import request from 'request';
import colors from 'colors'

export default function(file) {
    let conf = config.serverOptions,
        name = path.basename(file.path),
        dirName = path.dirname(file.path),
        relativePath = path.relative(global.rootPath, dirName),
        url = [
            conf.server,
            conf.removeScriptPath,
            '?user=',
            config.user.login,
            '&password=',
            config.user.password,
            '&path=',
            conf.projectPath,
            conf.themePath,
            '/',
            relativePath,
            '/',
            name,
            '&database=master'
        ].join('');
    setTimeout(function() {
        request.get({
            url: url,
            "rejectUnauthorized": false,
        }, function(err, httpResponse, body) {
            try {
                var response = JSON.parse(body);
                if (!response.result) {
                    console.log(('Error: ' + response.Reason).red);
                } else {
                    console.log('Removing successfull!'.green);
                }
                if (err) {
                    return console.error(('removing failed:').red, err.red);
                }
            } catch (e) {
                console.log(('Status code:' + httpResponse.statusCode).red);
                console.log(('Answer:' + httpResponse.body).red);
            }


        });
    }, 500)


}