namespace Sitecore.HabitatHome.Foundation.DependencyInjection.Infrastructure
{
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.DependencyInjection;

    public class MvcControllerServicesConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddMvcControllers("Sitecore.HabitatHome.Feature.*");
            serviceCollection.AddClassesWithServiceAttribute("Sitecore.HabitatHome.Feature.*");
            serviceCollection.AddClassesWithServiceAttribute("Sitecore.HabitatHome.Foundation.*");
        }
    }
}