# Feature: CRM #

The Habitat Home demo has a preconfigured integration with a Microsoft Dynamics CRM instance through a Data Exchange tenant. 

The Dynamics integration can be enabled/disabled in the Web.config by adding/removing "Dynamics" from the integrations:define app setting.

    <add key="integrations:define" value="Dynamics"/>

**If enabling Dynamics for the first time**, after updating the app setting, use the Unicorn.aspx dashboard to sync the necessary items for the feature

1. Go to https://habitathome.dev.local/unicorn.aspx
2. Select Feature.CRM
3. Click "Sync Selected"

You can validate that the Data Exchange tenant was created in Sitecore at this path - 
/sitecore/system/Data Exchange/Demo CRM

### How to connect the demo to a custom Microsoft Dynamics CRM ###
The connection string that the Demo CRM Data Exchange tenant expects is "democrm". In the ConnectionStrings.config file, add a connection string to your CRM instance with the same name (or replace the existing one if already present)

    <add name="democrm"
      	connectionString="url=[URL];user id=[USERID];password=[PASSWORD];organization=[ORGANIZATION];authentication type=2"/>


### How to start a scheduled task for automatic contact sync###
1. Navigate to /sitecore/system/Tasks/Schedules/CRM Contact Sync Schedule
2. Right-click and Insert a new "CRM Contact Sync Schedule". This will insert a pre-configured schedule item, which will run the necessary Data Exchange commands for syncing contacts every 10 minutes.
3. You can customize the schedule as necessary and delete the schedule item when done

