import gulp from "gulp";
import sass from 'gulp-sass';
import sourcemaps from 'gulp-sourcemaps';
import rename from 'gulp-rename';
import concat from 'gulp-concat';
import autoprefixer from 'gulp-autoprefixer';
import bulkSass from 'gulp-sass-bulk-import';
import gulpif from 'gulp-if';
//
import config from '../config';
import destPath from './destPath';

export default function(origFile) {
    let conf = config.sass.styles;
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
        .pipe(rename({ dirname: '' }))
        .pipe(concat(conf.concatName))
        .pipe(gulpif(function() {
            return config.sassSourceMap;
        }, sourcemaps.write('./')))
        .pipe(gulp.dest(function(file) { return destPath({ origFile: origFile, file: file }) }));
};