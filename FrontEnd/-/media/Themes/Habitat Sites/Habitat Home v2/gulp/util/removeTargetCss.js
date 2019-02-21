import gulp from "gulp";
import sass from 'gulp-sass';
import sourcemaps from 'gulp-sourcemaps';
import rename from 'gulp-rename';
import concat from 'gulp-concat';
import autoprefixer from 'gulp-autoprefixer';
import bulkSass from 'gulp-sass-bulk-import';
import gulpif from 'gulp-if';
import fs from 'fs';
import path from 'path';
//
import config from '../config';

export default function(filePath) {
    var targetPath = filePath.replace('sass', 'styles').replace('.scss', '.css');
    if (fs.existsSync(targetPath)) {
        fs.unlinkSync(targetPath)
    }

};