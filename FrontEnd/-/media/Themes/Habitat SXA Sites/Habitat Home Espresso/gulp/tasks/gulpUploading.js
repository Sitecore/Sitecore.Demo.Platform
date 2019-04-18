import gulp from 'gulp';
import sass from 'gulp-sass';
import watch from 'gulp-watch';
//
import login from './login';
import config from '../config';
import fileActionResolver from '../util/fileActionResolver';
//

gulp.task('upload-gulp', ['login'], () => {
    return gulp.src('gulp/**/*.js')
        .pipe(gulp.dest(function(file) {
            fileActionResolver(file, true);
            return './';
        }))
});
gulp.task('watch-gulp', ['login'], () => {
    return watch(['gulp/**/*.js', 'gulp/**/*.json'], function(file) {
        fileActionResolver(file);
    })
});