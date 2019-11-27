import gulp from 'gulp';
import cleanCSS from 'gulp-clean-css';

import gulpSourcemaps from 'gulp-sourcemaps';
import gulpif from 'gulp-if';
import gulpRename from 'gulp-rename';

import gulpConcat from 'gulp-concat';
import config from '../config';
export default function () {
    let conf = config.css;
    let streamSource = conf.minificationPath.concat(['!' + conf.cssOptimiserFilePath + conf.cssOptimiserFileName])
    return gulp.src(streamSource)
        .pipe(gulpif(function () {
            return conf.cssSourceMap;
        }, gulpSourcemaps.init()))
        .pipe(cleanCSS(config.minifyOptions.css))
        .pipe(gulpif(function () {
            return conf.jsSourceMap;
        }, gulpSourcemaps.write('./')))

        .pipe(gulpConcat(conf.cssOptimiserFileName))
        .pipe(gulp.dest('styles'));

}