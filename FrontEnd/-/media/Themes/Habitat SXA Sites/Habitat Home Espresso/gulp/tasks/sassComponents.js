import gulp from "gulp";
//
import componentsBuilder from '../util/sassComponentsBuilder';

gulp.task('sassComponents', ['login'], () => {
    return componentsBuilder()
});