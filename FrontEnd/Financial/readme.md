# Boilerplate for creating new theme for you Sitecore site

## For using Autosynchronizer, you need to complete next steps

1. Download theme boilerplate;
2. Open *PathToInstance/Website/App_Config/Include/z.Feature.Overrides* (in previous version of sitecore it can be *PathToInstance/Website/App_Config/Include/Feature*) folder and remove **.disabled** from **z.SPE.Sync.Enabler.Gulp.config.disabled** file;
3. Switch to downloaded theme boilerplate folder
4. Update config file for Gulp tasks. **ThemeRoot/gulp/config.js** file:
   1. `serverOptions.server` - path to sitecore instance. Example `server: 'http://sxa'`;
   2. `loginQuestions[0].default` - Your admin user name.
   3. `loginQuestions[1].default` - Your admin user password.
   4. `user.login` - Your admin user name.
   5. `user.password` - Your admin user password.
5. If you use Creative exchange skip this step. Open **ThemeRoot/gulp/serverConfig.json**
   1. `serverOptions.projectPath` - path to project, where theme placed. Example `projectPath: '\\themes'`;
   2. `serverOptions.themePath` - path to basic theme folder from project root. Example `themePath: '\\Basic2'`;
6. Open Theme root folder with command line.
7. Ensure you are using **Node.js 10.X**. This solution does not work with newer Node.js versions.
8. Run `npm install` (*node.js and npm should be already installed*);
9. If gulp is not yet installed - Install gulp using following command: `npm install --global gulp-cli`
10. Run `npm run start`
11. When watcher starts you need to enter you login and password for Sitecore, for uploading reason.
