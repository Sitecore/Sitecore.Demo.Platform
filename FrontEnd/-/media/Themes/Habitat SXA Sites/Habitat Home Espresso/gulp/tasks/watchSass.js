import gulp from 'gulp';
import sass from 'gulp-sass';
import sourcemaps from 'gulp-sourcemaps';
import rename from 'gulp-rename';
import concat from 'gulp-concat';
import autoprefixer from 'gulp-autoprefixer';
import watch from 'gulp-watch';
import path from 'path';
import fs from 'fs';
import sassGrapher from 'gulp-sass-grapher';
import filter from "gulp-filter";
import bulkSass from 'gulp-sass-bulk-import';
import gulpif from 'gulp-if';
import colors from 'colors'
//
import destPath from '../util/destPath';
import login from './login';
import config from '../config';
import stylesBuilder from '../util/sassStyleBuilder';
import componentsBuilder from '../util/sassComponentsBuilder';
import updateFromDependency from '../util/sassUpdateFromDependency';
import removeTargetCss from '../util/removeTargetCss';
import startTask from '../util/startTask';

gulp.task('watch-styles', () => {
    let conf = config.sass.styles;
    return watch(conf.sassPath, function(file) {
        stylesBuilder(file);
    })

})

gulp.task('watch-base', () => {
    let conf = config.sass.core;
    return watch(conf.sassPath, function(file) {
        stylesBuilder(file);
        componentsBuilder(file);
    })
})

gulp.task('watch-component', () => {
    var conf = config.sass.components;
    return watch(conf.sassPath, { base: path.resolve('sass/') })
        .on('unlink', function(file) {
            removeTargetCss(file);
        })
        .pipe(gulpif(function(file) {
            return file.event == 'change' || file.event == 'add';
        }, bulkSass()))
        .pipe(gulpif(function() {
            return config.sassSourceMap;
        }, sourcemaps.init()))
        .pipe(sass({ outputStyle: 'expanded' }).on('error', function(error) {
            startTask('watch-component', 0)
            sass.logError.call(this, error);

        }))
        .pipe(autoprefixer(config.autoprefixer))
        .pipe(rename({ dirname: '' }))
        .pipe(gulpif(function() {
            return config.sassSourceMap;
        }, sourcemaps.write()))
        .pipe(gulp.dest(function(file) { return destPath({ file: file }) }));

});

gulp.task('watch-dependency', () => {
    let conf = config.stylesConfig;
    return watch(['sass/styles/**/*.scss','sass/variants/**/*.scss'], { base: path.resolve('sass/') }, function(file) {
        let dirName = file.dirname.match('[a-z\-]*$')[0],
            fileName = conf[dirName];
        if (fileName) {
            updateFromDependency(file, fileName);
        }

    })
})



gulp.task('sass-watch',
    function() {
        setTimeout(function() {
            console.log('Watching SASS files started...'.green);
        }, 0);
        global.isWatching = true;
        gulp.run('watch-component')
        gulp.run('watch-base')
        gulp.run('watch-styles')
        gulp.run('watch-dependency')
    })