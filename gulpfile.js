var gulp = require("gulp");
var fs = require("fs");
var unicorn = require("./scripts/unicorn.js");
var habitat = require("./scripts/habitat.js");
var helix = require("./scripts/helix.js");
var runSequence = require("run-sequence");
var nugetRestore = require("gulp-nuget-restore");

var config;
if (fs.existsSync("./gulp-config.js.user")) {
    config = require("./gulp-config.js.user")();
} else {
    config = require("./gulp-config.js")();
}

module.exports.config = config;

helix.header("The Habitat source code, tools and processes are examples of Sitecore Helix.",
    "Habitat is not supported by Sitecore and should be used at your own risk.");

gulp.task("default",
    function (callback) {
        config.runCleanBuilds = true;
        return runSequence(
            "Copy-Sitecore-License",
            "Copy-Sitecore-Lib",
            "Nuget-Restore",
            /*,
            "Publish-All-Projects",
            "Apply-Xml-Transform",
            "Sync-Unicorn",
            "Publish-Transforms",*/
            callback);
    });

/*****************************
  Initial setup
*****************************/
gulp.task("Copy-Sitecore-License",
    function () {
        console.log("Copying Sitecore License file");
        return gulp.src(config.licensePath).pipe(gulp.dest("./lib"));
    });

gulp.task("Copy-Sitecore-Lib",
    function () {
        console.log("Copying Sitecore Libraries");

        fs.statSync(config.sitecoreLibraries);

        var files = config.sitecoreLibraries + "/**/*";

        return gulp.src(files).pipe(gulp.dest("./lib/Sitecore"));
    });

gulp.task("Nuget-Restore",
    function (callback) {
        var solution = "./" + config.solutionName + ".sln";
        return gulp.src(solution).pipe(nugetRestore());
    });