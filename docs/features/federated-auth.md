# Feature: Federated Authentication #
The Habitat Home Demo uses Sitecore's Federated Authentication feature to showcase logging in with external autentication providers.

## How to enable existing 3rd party integrations

The demo currently supports Federated Auth with Facebook and with Microsoft Account.
To enable the integration with Facebook, modify this appSetting in the Web.config


    <add key="integrations:define" value="Facebook"/>


To enable the integration with Microsoft, modify this appSetting in the Web.config

    <add key="integrations:define" value="MS"/>

Both can be enabled at the same time by comma-separating the values in the appSetting

## Facebook ##
To configure authentication with Facebook, you will need to create a Facebook app through the Facebook developer portal. For more information, visit [https://developers.facebook.com](https://developers.facebook.com)

Once an app is registered with Facebook, modify the following settings in the *~/App_Config/Include/Feature/Feature.Accounts.config* (or you can patch them in a local patch config file)

            <setting name="Sitecore.HabitatHome.Feature.Accounts.Facebook.AppId" integrations:require="Facebook" value="YOUR-APP-ID"/>
            <setting name="Sitecore.HabitatHome.Feature.Accounts.Facebook.AppSecret" integrations:require="Facebook" value="YOUR-APP-SECRET"/>


## Microsoft Account ##
To configure authentication with Microsoft Account, you will need to create an MS app through the MS developer portal here - [https://apps.dev.microsoft.com/#/appList](https://apps.dev.microsoft.com/#/appList)

Once an app is registered with Microsoft, modify the following settings in the *~/App_Config/Include/Feature/Feature.Accounts.config* (or you can patch them in a local patch config file)

            <setting name="Sitecore.HabitatHome.Feature.Accounts.Microsoft.ClientId" integrations:require="MS" value="YOUR-CLIENT-ID"/>
            <setting name="Sitecore.HabitatHome.Feature.Accounts.Microsoft.ClientSecret" integrations:require="MS" value="YOUR-CLIENT-SECRET"/>