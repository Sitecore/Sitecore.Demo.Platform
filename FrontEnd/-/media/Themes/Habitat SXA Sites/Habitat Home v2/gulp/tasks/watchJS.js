import gulp from 'gulp';
import sass from 'gulp-sass';
import watch from 'gulp-watch';
import gulpif from 'gulp-if';
import eslint from 'gulp-eslint';
import colors from 'colors'
//
import login from './login';
import config from '../config';
import fileActionResolver from '../util/fileActionResolver';

gulp.task('watch-js', ['login'], () => {
    var conf = config.js;
    setTimeout(function() {
        console.log('Watching JS files started...'.green);
    }, 0);
    return watch(conf.path, { verbose: 0 }, function(file) {
        var stream = gulp.src(file.path)
            .pipe(eslint())
            .pipe(eslint.format())
            .pipe(eslint.results(results => {
                // Called once for all ESLint results.
                if (results.errorCount == 0 || conf.esLintUploadOnError) {
                    fileActionResolver(file);
                } else {
                    console.log('Please fix errors before uploading or set esLintUploadOnError:true'.yellow);
                }
            }));


        return stream;
    })
});

gulp.task('js-watch', ['login'],
    function() {
        global.isWatching = true;
        gulp.run('watch-js')
    })