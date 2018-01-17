var gulp = require("gulp");
var fs = require("fs");
var unicorn = require("./scripts/unicorn.js");
var habitat = require("./scripts/habitat.js");
var helix = require("./scripts/helix.js");
var runSequence = require("run-sequence");
var nugetRestore = require("gulp-nuget-restore");
var msbuild = require("gulp-msbuild");
var foreach = require("gulp-foreach");
var debug = require("gulp-debug");
var util = require("gulp-util");

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
            "Publish-All-Projects",
            "Apply-Xml-Transform",
            "Publish-Transforms",
            callback);
    });

gulp.task("deploy-unicorn",
    function (callback) {
        config.runCleanBuilds = true;
        return runSequence(
            "Copy-Sitecore-License",
            "Copy-Sitecore-Lib",
            "Nuget-Restore",
            "Publish-All-Projects",
            "Apply-Xml-Transform",
            "Sync-Unicorn",
            "Publish-Transforms",
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

gulp.task("Publish-All-Projects",
    function (callback) {
        return runSequence(
            "Build-Solution",
            "Publish-Foundation-Projects",
            "Publish-Feature-Projects",
            "Publish-Project-Projects",
            
            callback);
    });


gulp.task("Apply-Xml-Transform",
    function () {
        var layerPathFilters = [
            "./src/Foundation/**/*.xdt", "./src/Feature/**/*.xdt", "./src/Project/**/*.xdt",
            "!./src/**/obj/**/*.xdt", "!./src/**/bin/**/*.xdt"
        ];
        return gulp.src(layerPathFilters)
            .pipe(foreach(function (stream, file) {
                var fileToTransform = file.path.replace(/.+code\\(.+)\.xdt/, "$1");
                util.log("Applying configuration transform: " + file.path);
                return gulp.src("./scripts/applytransform.targets")
                    .pipe(msbuild({
                        targets: ["ApplyTransform"],
                        configuration: config.buildConfiguration,
                        logCommand: false,
                        verbosity: config.buildVerbosity,
                        stdout: true,
                        errorOnFail: true,
                        maxcpucount: config.buildMaxCpuCount,
                        nodeReuse: false,
                        toolsVersion: config.buildToolsVersion,
                        properties: {
                            Platform: config.buildPlatform,
                            WebConfigToTransform: config.websiteRoot,
                            TransformFile: file.path,
                            FileToTransform: fileToTransform
                        }
                    }));
            }));
    });

gulp.task("Sync-Unicorn",
    function (callback) {
        var options = {};
        options.siteHostName = habitat.getSiteUrl();
        options.authenticationConfigFile = config.websiteRoot + "/App_config/Include/Unicorn.SharedSecret.config";
        options.maxBuffer = "Infinity";

        unicorn(function () { return callback() }, options);
    });


gulp.task("Publish-Transforms",
    function () {
        return gulp.src("./src/**/code/**/*.xdt")
            .pipe(gulp.dest(config.websiteRoot + "/temp/transforms"));
    });


gulp.task("Build-Solution",
    function () {
        var targets = ["Build"];
        if (config.runCleanBuilds) {
            targets = ["Clean", "Build"];
        }

        var solution = "./" + config.solutionName + ".sln";
        return gulp.src(solution)
            .pipe(msbuild({
                targets: targets,
                configuration: config.buildConfiguration,
                logCommand: false,
                verbosity: config.buildVerbosity,
                stdout: true,
                errorOnFail: true,
                maxcpucount: config.buildMaxCpuCount,
                nodeReuse: false,
                toolsVersion: config.buildToolsVersion,
                properties: {
                    Platform: config.buildPlatform
                }
            }));
    });


/*****************************
  Publish
*****************************/
var publishStream = function (stream, dest) {
    var targets = ["Build"];

    return stream
        .pipe(debug({ title: "Building project:" }))
        .pipe(msbuild({
            targets: targets,
            configuration: config.buildConfiguration,
            logCommand: false,
            verbosity: config.buildVerbosity,
            stdout: true,
            errorOnFail: true,
            maxcpucount: config.buildMaxCpuCount,
            nodeReuse: false,
            toolsVersion: config.buildToolsVersion,
            properties: {
                Platform: config.publishPlatform,
                DeployOnBuild: "true",
                DeployDefaultTarget: "WebPublish",
                WebPublishMethod: "FileSystem",
                BuildProjectReferences: "false",
                DeleteExistingFiles: "false",
                publishUrl: dest
            }
        }));
};
var publishProjects = function (location, dest) {
    dest = dest || config.websiteRoot;

    console.log("publish to " + dest + " folder");
    return gulp.src([location + "/**/code/*.csproj"])
        .pipe(foreach(function (stream, file) {
            return publishStream(stream, dest);
        }));
};
gulp.task("Publish-Foundation-Projects",
    function () {
        return publishProjects("./src/Foundation");
    });
gulp.task("Publish-Feature-Projects",
    function () {
        return publishProjects("./src/Feature");
    });

gulp.task("Publish-Project-Projects",
    function () {
        return publishProjects("./src/Project");
    });
