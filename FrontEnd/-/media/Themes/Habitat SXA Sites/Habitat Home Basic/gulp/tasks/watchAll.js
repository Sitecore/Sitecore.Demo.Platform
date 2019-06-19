import gulp from 'gulp';
import sass from 'gulp-sass';
import watch from 'gulp-watch';
import fs from 'fs';
//
import login from './login';


gulp.task('all-watch', ['login'],
    function() {
        global.isWatching = true;
        gulp.run('sass-watch')
        gulp.run('js-watch')
        gulp.run('css-watch')
        gulp.run('img-watch')
        gulp.run('watch-source-sass')
        gulp.run('html-watch')
    })

gulp.task('default', ['all-watch'])