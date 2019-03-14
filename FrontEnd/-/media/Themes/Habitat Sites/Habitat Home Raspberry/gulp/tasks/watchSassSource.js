import gulp from 'gulp';
import sass from 'gulp-sass';
import watch from 'gulp-watch';
//
import login from './login';
import config from '../config';
import fileActionResolver from '../util/fileActionResolver';

gulp.task('watch-source-sass', ['login'], () => {
    var conf = config.sass;
    return watch(conf.root, function(file) {
        fileActionResolver(file);
    })
});

gulp.task('sass-source-watch', ['login'],
    function() {
        global.isWatching = true;
        gulp.run('watch-source-sass')
    })