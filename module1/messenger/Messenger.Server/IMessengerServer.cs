using System.Threading.Tasks;

namespace Messenger.Server
{
	public interface IMessengerServer
	{
		Task StartAsync();
		void Stop();
	}
}
