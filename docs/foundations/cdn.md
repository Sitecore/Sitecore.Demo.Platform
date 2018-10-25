# Foundation: CDN #

The Habitat Home Content demo has preconfigured configuration with a Microsoft Azure CDN that is **DISABLED** by default.

**Before enabling the CDN functionality on HabitatHome you must set up your own Azure CDN profile.**

A CDN provider should not be shared amongst multiple websites or users so you are required to set up your own CDN provider. Please see the instructions below that outline how to set up an Azure CDN profile on your Azure subscription.

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
      
- The CDN functionality can be enabled by renaming adding the "CDN" parameter to the following setting in the Web.config file:

<add key="integrations:define" value="CDN"/>

* Note that this setting may have other values than just CDN.

## Degugging Help ##

- Azure CDN endpoints do not serve assets if a website is running a non-secure SSL certificate. You will see either a 404 or 503 response in this situation. This can make it difficult to use a public IP address for your demo site as you will need a SSL cert.