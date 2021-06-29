namespace Sitecore.Demo.Init.Services
{
	using System.Threading.Tasks;

	public interface IStateService
	{
		Task SetState(string status);
	}
}
