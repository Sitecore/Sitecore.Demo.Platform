namespace Sitecore.Demo.Init.Services
{
	using System.IO;
	using System.Threading.Tasks;

	class StateService : IStateService
	{
		public async Task SetState(string status)
		{
			await File.WriteAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), $"app.state"), status);
		}
	}
}
