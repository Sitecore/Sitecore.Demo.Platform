using Microsoft.Extensions.Logging;

namespace Sitecore.Demo.Init.Extensions
{
	internal static class ApplicationLogging
	{
		internal static ILoggerFactory LoggerFactory { get; set; }
		internal static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();
		internal static ILogger CreateLogger(string categoryName) => LoggerFactory.CreateLogger(categoryName);
	}
}
