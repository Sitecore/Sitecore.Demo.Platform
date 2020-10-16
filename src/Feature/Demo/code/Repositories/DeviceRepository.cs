using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.CES.DeviceDetection;
using Sitecore.Demo.Platform.Feature.Demo.Models;
using Sitecore.Demo.Platform.Foundation.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Sitecore.Demo.Platform.Feature.Demo.Repositories
{
	[Service]
    public class DeviceRepository
    {
        private DeviceDetectionManagerBase deviceDetectionManager => ServiceLocator.ServiceProvider.GetRequiredService<DeviceDetectionManagerBase>();

        public Device GetCurrent()
        {
	        if (!deviceDetectionManager.IsEnabled || !deviceDetectionManager.IsReady || string.IsNullOrEmpty(Tracker.Current.Interaction.UserAgent))
            {
                return null;
            }

			return this.CreateDevice(deviceDetectionManager.GetDeviceInformation(Tracker.Current.Interaction.UserAgent));
        }

        private Device CreateDevice(DeviceInformation deviceInformation)
        {
            return new Device
            {
                Title = string.Join(", ", deviceInformation.DeviceVendor, deviceInformation.DeviceModelName),
                Browser = deviceInformation.Browser
            };
        }
    }
}
