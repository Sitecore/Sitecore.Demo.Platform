const {gulpTaskInit} = require('@sxa/celt');
global.rootPath = __dirname;

//	Ensure process ends after all Gulp tasks are finished

gulpTaskInit();

const gulp = require("gulp");
const cssMinificator = require('@sxa/celt/util/cssMinificator');
gulp.task('sassMinify', function() {
    cssMinificator();
});