using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Sitecore.Demo.Platform.Foundation.DependencyInjection.Infrastructure
{
    public class MvcControllerServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcControllers("Sitecore.Demo.Platform.Feature.*");
            serviceCollection.AddClassesWithServiceAttribute("Sitecore.Demo.Platform.Feature.*");
            serviceCollection.AddClassesWithServiceAttribute("Sitecore.Demo.Platform.Foundation.*");
        }
    }
}