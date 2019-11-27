import config from '../config';
import path from 'path';
import gulp from 'gulp';
import urllib from 'urllib';
import formstream from 'formstream';
import request from 'request';
import formData from 'form-data';
import * as fs from 'fs';
import querystring from 'querystring';
import colors from 'colors'

export default function(file) {
    var conf = config.serverOptions,
        name = path.basename(file.path),
        dirName = path.dirname(file.path),
        relativePath = path.relative(global.rootPath, dirName),
        prom = new Promise((resolve, reject) => {
            setTimeout(function() {
                resolve();
            }, 600);
            let url = [conf.server,
                conf.updateTemplatePath,
                '?user=',
                config.user.login,
                '&password=',
                config.user.password,
                '&path=',
                name
            ].join('');
            var formData = {
                file: fs.createReadStream(relativePath + '/' + name),
            };
            console.log(url);
            var a = request.post({
                url: url,
                strictSSL: false,
                formData: formData,
                agentOptions : {
                    rejectUnauthorized :false
                }
            }, function(err, httpResponse, body) {
                resolve();
                if (err) {
                    return console.log(('upload failed:' + err).red);
                }
                if (httpResponse.statusCode !== 200) {
                    console.log(('Status code:' + httpResponse.statusCode).red);
                    console.log(('Answer:' + httpResponse.body).red);
                    return
                } else {
                    return console.log(('Uploading of ' + name + ' was successful!').green);
                }


            });

        });
    return prom;
}