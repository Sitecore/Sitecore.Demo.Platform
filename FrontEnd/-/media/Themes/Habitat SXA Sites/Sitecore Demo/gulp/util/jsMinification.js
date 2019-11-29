import gulp from 'gulp';
import gulpConcat from 'gulp-concat';
import gulpUglify from 'gulp-uglify';
import gulpSourcemaps from 'gulp-sourcemaps';
import gulpif from 'gulp-if';
import babel from "gulp-babel";

import gulpRename from 'gulp-rename';
//
import config from '../config';
import destPath from './destPath';

export default function () {
    let conf = config.js;
    let streamSource = conf.minificationPath.concat(['!' + conf.jsOptimiserFilePath + conf.jsOptimiserFileName])
    var stream = gulp
        .src(streamSource)
        .pipe(gulpif(function () {
            return conf.es6Support;
        }, babel()))
        .pipe(gulpif(function () {
            return conf.jsSourceMap;
        }, gulpSourcemaps.init()))
        .pipe(gulpConcat(conf.jsOptimiserFileName))
        .pipe(gulpUglify(config.minifyOptions.js))
        .pipe(gulpif(function () {
            return conf.jsSourceMap;
        }, gulpSourcemaps.write('./')))
        .pipe(gulp.dest('scripts'));
    return stream;
};