import gulp from "gulp";
import spritesmith from 'gulp.spritesmith';
import config from '../config';

gulp.task('sprite-flag', function() {
    let conf = config.sprites.flags,
        spriteData = gulp.src(conf.flagsFolder).pipe(spritesmith(conf.spritesmith));
    spriteData.img.pipe(gulp.dest(conf.imgDest));
    return spriteData.css.pipe(gulp.dest(conf.cssDest));
});