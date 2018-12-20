# BackstopJS Integration
This project uses BackstopJS and its screenshot comparison and report
generation functionality to determine if test steps pass or fail.  NUnit
tests using Selenium automate browser interaction and generate screenshots
of the end result while BackstopJS compares test results with earlier runs
to determine if the results match.

----

## Setup
1. Install [NodeJS](https://nodejs.org/en/download/) (aka uninstall your
old NodeJS and install the newest one) *
2. In `cmd.exe` (as Administrator), run the following commands...
    * > `CD \projects\Sitecore.HabitatHome.Platform\src\Project\HabitatHome\tests\backstop`
    * > `npm install -g backstopjs`
    * > `backstop init`

\* Seriously though, you need to do this.

----

## Usage
1. Run tests in Visual Studio
    * click **Test > Windows > Test Explorer** in menu bar
    * click **Run All**
3. Run `node backstopReport.js` in Command Line
    * run **cmd.exe**
    * goto backstop directory
    `\projects\Sitecore.HabitatHome.Platform\src\Project\HabitatHome\tests\backstop`
    * run `node backstopReport.js`

## Approving tests
In cases where tests are failing due to changes that need to be retained, and you want to update the test images with the changes

1. Run `backstop approve` in Command Line
	* run **cmd.exe**
	* goto backstop directory
    `\projects\Sitecore.HabitatHome.Platform\src\Project\HabitatHome\tests\backstop`
    * run `backstop approve`
