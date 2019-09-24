##
Boilerplate for creating new theme for you Sitecore site. 

## For using Autosynchronizer, you need to complete next steps:

1. Download theme boilerplate;
2. Open *PathToInstance/Website/App_Config/Include/z.Feature.Overrides* (in previous version of sitecore it can be *PathToInstance/Website/App_Config/Include/Feature*) folder and remove **.disabled** from **z.SPE.Sync.Enabler.Gulp.config.disabled** file;
3. Switch to downloaded theme boilerplate folder
4. Update config file for Gulp tasks. **ThemeRoot/gulp/config.js** file:
    1. `serverOptions.server` - path to sitecore instance. Example `server: 'http://sxa'`;
6. If you use Creative exchange skip this step. Open **ThemeRoot/gulp/serverConfig.json** 
     1. `serverOptions.projectPath` - path to project, where theme placed. Example ` projectPath: '/themes'`;
    2. `serverOptions.themePath` - path to basic theme folder from project root. Example ` themePath: '/Basic2'`;
5. Open Theme root folder with command line.
6. Run `npm install` (*node.js and npm should be already installed*);
7. If gulp is not yet installed - Install gulp using following command: `npm install --global gulp-cli` 
8. Run gulp task which you need: <br/>
    Global tasks:
    1. `gulp default` or just `gulp` - starts `gulp all-watch`.
    2. `gulp all-watch` - run a list of tasks:<br/>
            `sass-watch`<br/>
            `js-watch`<br/>
            `es-watch`<br/>
            `css-watch`<br/>
            `img-watch`<br/>
            `watch-source-sass`<br/>
            `html-watch`<br/>

    For SASS
    1. `gulp sass-watch` - run a list of tasks:
        `watch-component`
        `watch-base`
        `watch-styles`
        `watch-dependency`
    1. `gulp sassComponents` - to compile sass styles just for components;
    2. `gulp sassStyles` - to compile sass additional styles for component;
    3. `gulp watch-styles` - watch changes under **sass/styles/common** , **sass/styles/content-alignment** , **sass/styles/layout** folders and compile all of them to **styles/styles.css**;
    4. `gulp watch-base` - watch on changes under  **sass/abstracts/**, **sass/base/** , **sass/components** folders and run compiling of components and styles;
    5. `gulp watch-component` - watch changes of component styles under *sass* folder and compile them to **styles** folder;
    6. `gulp watch-dependency` - watch changes under **sass/styles/** (exluded **sass/styles/common** , **sass/styles/content-alignment** , **sass/styles/layout**) and compile appropriate component;

    For CSS
    1. `gulp css-watch` - watch on changes of css files under **stytles** folder and upload them to server;

    For JavaScript:
    1. `gulp eslint` - run eslint for all js in **scripts** folder;
    2. `gulp js-watch` - watch on changes of js files under **scripts** folder and upload them to server;
    2. `gulp es-watch` - watch on changes of ES6+ js files under **sources** folder and upload them to server;
   
    For HTML (if you work with creative exchange)
    1. `gulp html-watch` - watch changes of html files and upload them to the server;

    For Gulp files
    1. `gulp watch-gulp` - watch on changes of js and json files under **gulp** folder and upload them to server;

    For Images
    1. `gulp img-watch` - watch on changes under **images** folder and upload files to server;

    For Sprite
    1. `gulp spriteFlag` - to create sprite for flags;

9. When watcher starts you need to enter you login and password for sitecore, for uploading reason.

