# Foundation: CDN #

The Habitat Home Content demo has preconfigured configuration with a Microsoft Azure CDN that is **DISABLED** by default.

**Before enabling the CDN functionality on HabitatHome you must set up your own Azure CDN profile.**

A CDN provider cannot be shared amongst multiple websites or users so you are required to set up your own CDN provider. Please see the instructions below that outline how to set up an Azure CDN profile on your Azure subscription.

- You will require your own Microsoft Azure subscription to set up a unique CDN profile for your usage.
- Please follow the attached guide in order to set up an Azure CDN: *https://docs.microsoft.com/en-us/azure/cdn/cdn-create-new-endpoint*
    - Make note of the following CDN setting during setup:
	    - **Endpoint hostname** - Can be a public IP address or public domain name
- Important! - Ensure the Azure CDN endpoint has the following "*Caching rules*" setting configured:
  - Query string caching behavior: **Cache every unique URL**
- Set the "*Media.MediaLinkServerUrl*" setting in the "*Foundation.CDN.config.disabled*" config file to the Azure CDN endpoint you created in the previous step.

      <setting name="Media.MediaLinkServerUrl" >
        <patch:attribute name="value">https://[yourAzureCdnEndpoint].azureedge.net</patch:attribute>
      </setting>
      
- The CDN functionality can be enabled by renaming the "*Foundation.CDN.config.disabled*" config file to "*Foundation.CDN.config*"
- NOTE: To disable the CDN, please rename this file to .disabled. The CDN.Enabled setting is not a feature switch for all CDN related settings in this file!