# How to Add a New Site

#### Clone An Existing Site

Right click the site you wish to clone. Click "Duplicate" and enter the name of your new site.

#### Create a New Site

1) Right click on /sitecore/content/Habitat SXA Sites

2) Insert a new Site

3) Under the "Modules" tab, select the modules you wish the new site to contain. See other documentation in this folder for details on various modules.

4) Under the "Theme" tab, select a theme for the new site.

5) Click "Ok".

#### Post-Steps

Once a new site is created or duplicated, the following post-steps should be performed:

1) Navigate to 

/sitecore/content/Habitat SXA Sites/{Your Site Name}/Settings/Site Grouping and insert a new Site or click on the existing child site. 

2) Bind a new site to IIS (see below) and set this item to the hostname.

#### Bind a New Site to IIS

1) Open up a Powershell window. Navigate to the xp/install folder inside the Sitecore.HabitatHome.Utilities project.

`cd C:\Projects\Sitecore.HabitatHome.Utilities\XP\install`

2) Run `Add-SSLSiteBindingWithCertificate.ps1` with your desired hostname.

`.\Add-SSLSiteBindingWithCertificate.ps1 -$SiteName habitathome.dev.local -$HostName yourhostname.dev.local -$CertificatName yourhostname.dev.local `