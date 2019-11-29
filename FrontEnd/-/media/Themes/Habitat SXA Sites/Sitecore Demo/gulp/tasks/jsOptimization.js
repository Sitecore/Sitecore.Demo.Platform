import gulp from 'gulp';
import jsMinificator from '../util/jsMinification';

gulp.task('jsOptimise', function () {
    return jsMinificator();
});
