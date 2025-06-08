using System.Net.WebSockets;
using System.Text;

namespace AIGCImageHistory
{
	public class CheckProgressHelper
	{
		private ClientWebSocket _client;

		public async Task ConnectAsync(string url,string clientId)
		{
			_client = new ClientWebSocket();
			string ws_url = url;
			string client_id = clientId;
			string fullUrl = $"{ws_url}/ws?client_id={client_id}";
			await _client.ConnectAsync(new Uri(fullUrl), CancellationToken.None);
		}
		public async Task<string> ReceiveMessageAsync()
		{
			var receiveBuffer = new byte[1024];
			var result = await _client.ReceiveAsync(new ArraySegment<byte>(receiveBuffer),
				CancellationToken.None);

			var message = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
			Console.WriteLine("Received: {0}", message);
			return message;
		}
	}
}
