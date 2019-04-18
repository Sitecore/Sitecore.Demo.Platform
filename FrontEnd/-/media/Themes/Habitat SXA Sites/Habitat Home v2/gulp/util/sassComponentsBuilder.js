import gulp from "gulp";
import sass from 'gulp-sass';
import sourcemaps from 'gulp-sourcemaps';
import autoprefixer from 'gulp-autoprefixer';
import debug from 'gulp-debug';
import bulkSass from 'gulp-sass-bulk-import';
import gulpif from 'gulp-if';
//
import destPath from '../util/destPath';
import config from '../config';
import login from '../tasks/login';

export default function(origFile) {
    var conf = config.sass.components;
    return gulp.src(conf.sassPath)
        .pipe(gulpif(function(file) {
            return typeof origFile == 'undefined' || origFile.event == 'change' || origFile.event == 'add';
        }, bulkSass()))
        .pipe(gulpif(function() {
            return config.sassSourceMap;
        }, sourcemaps.init()))
        .pipe(sass({ outputStyle: 'expanded' }).on('error', function(error) {
            sass.logError.call(this, error);

        }))
        .pipe(autoprefixer(config.autoprefixer))
        .pipe(gulpif(function() {
            return config.sassSourceMap;
        }, sourcemaps.write()))
        .pipe(gulp.dest(function(file) { return destPath({ origFile: origFile, file: file }) }))
};