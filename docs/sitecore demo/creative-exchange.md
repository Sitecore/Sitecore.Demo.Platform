# Creative Exchange

As you edit SASS files, it is possible to have those files compiled to CSS and uploaded to Sitecore automatically. To do so:

1. Enable `z.SPE.Sync.Enabler.Gulp.config.disabled` in the webroot (`C:\inetpub\wwwroot\habitathome.dev.local\App_Config\Include\z.Feature.Overrides`).

2. Navigate to the theme folder containing the files you wish to edit. Example:

```
cd C:\Projects\Sitecore.HabitatHome.Platform\FrontEnd\-\media\Themes\Habitat SXA Sites\Sitecore Demo
```

3. Run `npm install` if node modules are not already installed.

4. Run `.\node_modules\.bin\gulp`.

Please note:
 - Gulp 3 is incompatible with the latest version of Node. To use gulp, you will need to use node version 10 or lower.
 - If you are using a site binding other than `habitathome.dev.local`, you will need to update the "serverOptions -> server" parameter in the `gulp\config.js` file.