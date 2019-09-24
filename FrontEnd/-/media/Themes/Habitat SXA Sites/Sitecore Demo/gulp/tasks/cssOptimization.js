import gulp from 'gulp';
import cssMinificator from '../util/cssMinificator';

gulp.task('cssOptimise', () => {
    return cssMinificator()
})