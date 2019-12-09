import gulp from 'gulp';
import sass from 'gulp-sass';
import watch from 'gulp-watch';
import colors from 'colors'
//
import login from './login';
import config from '../config';

import { fileActionResolver, queueInstance } from '../util/fileActionResolver';
import cssMinificator from '../util/cssMinificator';
//

gulp.task('watch-css', ['login'], () => {
    var conf = config.css,
        indervalId;
    setTimeout(function () {
        console.log('Watching CSS files started...'.green);
    }, 0);
    return watch(conf.path, function (file) {
        if (!conf.disableSourceUploading || file.path.indexOf(conf.cssOptimiserFileName) > -1) {
            fileActionResolver(file);
        } else {
            console.log('Uploading prevented because value disableSourceUploading:true'.yellow);

        }
        if (conf.enableMinification && file.path.indexOf(conf.cssOptimiserFileName) == -1 && !indervalId) {
            indervalId = setInterval(function () {
                if (queueInstance.queueLenght() == 0) {
                    indervalId = clearInterval(indervalId);
                    cssMinificator();
                }
            }, 1500)
        }
    })
});

gulp.task('css-watch', ['login'],
    function () {
        global.isWatching = true;
        gulp.run('watch-css')
    })