var gulp = require("gulp");
var fs = require("fs");
var unicorn = require("./scripts/unicorn.js");
var habitat = require("./scripts/habitat.js");
var habitatHome = require("./scripts/habitat-home.js");
var runSequence = require("run-sequence");
var nugetRestore = require("gulp-nuget-restore");
var msbuild = require("gulp-msbuild");
var foreach = require("gulp-foreach");
var debug = require("gulp-debug");
var util = require("gulp-util");
var get = require("simple-get");

var config;
if (fs.existsSync("./gulp-config.user.js")) {
    config = require("./gulp-config.user.js")();
} else if (fs.existsSync("./gulp-config.sc-internal.js")) {
    config = require("./gulp-config.sc-internal.js")();
} else {
    config = require("./gulp-config.js")();
}

module.exports.config = config;

habitatHome.header("The Habitat Home source code, tools and processes are examples of Sitecore Features.",
    "Habitat Home is not supported by Sitecore and should be used at your own risk.");

gulp.task("default",
    function (callback) {
        config.runCleanBuilds = true;
        return runSequence(
            "Nuget-Restore",
            "Publish-All-Projects",
            "Apply-Xml-Transform",
            "Sync-Unicorn",
            "Publish-Transforms",
            "Publish-xConnect-Project",
            "Deploy-EXM-Campaigns",
            "Deploy-Marketing-Definitions",
            "Rebuild-Core-Index",
            "Rebuild-Master-Index",
            "Rebuild-Web-Index",
            callback);
    });

gulp.task("quick-deploy",
    function (callback) {
        config.runCleanBuilds = true;
        return runSequence(
            "Publish-All-Projects",
            "Apply-Xml-Transform",
            "Publish-Transforms",
            "Publish-xConnect-Project",
            callback);
    });


/*****************************
  Initial setup
*****************************/
gulp.task("Copy-Sitecore-Lib", function (callback) {
    console.log("Copying Sitecore XA Libraries");

    fs.statSync(config.sitecoreLibraries);
    const sxa = config.sitecoreLibraries + "/**/Sitecore.XA.*";
    return gulp.src(sxa).pipe(gulp.dest("./lib/Modules/SXA"));
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
                var fileToTransform = file.path.replace(/.+code\\(.+)\.xdt/, "$1")
                    .replace("\.sc-internal", "");
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
        options.maxBuffer = Infinity;

        unicorn(function () {
            return callback()
        }, options);
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
                },
            customArgs: ["/restore"]
            }));
    });

/*****************************
  Publish
*****************************/
var publishStream = function (stream, dest) {
    var targets = ["Build"];

    return stream
        .pipe(debug({
            title: "Building project:"
        }))
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
            },
            customArgs: ["/restore"]
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
        publishProjects("./src/Project/Common");
        publishProjects("./src/Project/Habitat");
        publishProjects("./src/Project/HabitatHome");
        return;
    });

gulp.task("Publish-xConnect-Project",
    function () {
        return publishProjects("./src/Project/xConnect", config.xConnectRoot);
    });

gulp.task("Deploy-EXM-Campaigns",
    function () {
        console.log("Deploying EXM Campaigns");

        var url = config.instanceUrl + "utilities/deployemailcampaigns.aspx?apiKey=" + config.messageStatisticsApiKey;
        console.log("Deploying EXM Campaigns at " + url);
        get({
            url: url,
            "rejectUnauthorized": false
        }, function (err, res) {
            if (err) {
                throw err;
            }
        });
    });

gulp.task("Deploy-Marketing-Definitions",
    function () {
        console.log("Deploying Marketing Definitions");

        var url = config.instanceUrl + "utilities/deploymarketingdefinitions.aspx?apiKey=" + config.marketingDefinitionsApiKey;
        console.log("Deploying Marketing Definitions at " + url);
        get({
            url: url,
            "rejectUnauthorized": false
        }, function (err, res) {
            if (err) {
                throw err;
            }
        });
    });

gulp.task("Rebuild-Core-Index",
    function () {
        console.log("Rebuilding Index Core");

        var url = config.instanceUrl + "utilities/indexrebuild.aspx?index=sitecore_core_index";

        get({
            url: url,
            "rejectUnauthorized": false
        }, function (err, res) {
            if (err) {
                throw err;
            }
        });
    });

gulp.task("Rebuild-Master-Index",
    function () {
        console.log("Rebuilding Index Master");

        var url = config.instanceUrl + "utilities/indexrebuild.aspx?index=sitecore_master_index";

        get({
            url: url,
            "rejectUnauthorized": false
        }, function (err, res) {
            if (err) {
                throw err;
            }
        });
    });

gulp.task("Rebuild-Web-Index",
    function () {
        console.log("Rebuilding Index Web");

        var url = config.instanceUrl + "utilities/indexrebuild.aspx?index=sitecore_web_index";

        get({
            url: url,
            "rejectUnauthorized": false
        }, function (err, res) {
            if (err) {
                throw err;
            }
        });
    });