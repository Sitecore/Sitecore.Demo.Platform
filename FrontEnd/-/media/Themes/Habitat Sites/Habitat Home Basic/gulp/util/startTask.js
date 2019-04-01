import gulp from 'gulp';

// Filters out non .js files. Prevents
// accidental inclusion of possible hidden files
export default function(taskName, timeOut) {
    if (typeof timeOut !== 'undefined' && timeOut >= 0) {
        setTimeout(function() {
            gulp.run(taskName)
        }, timeOut)
    } else {
        gulp.run(taskName)
    }
};