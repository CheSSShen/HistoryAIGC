using AIGCImageHistory.RealTimeTranscript;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

class RealTimeTranscription
{
	public ClientWebSocket client; //WebSocket连接客户端
	private string URL;
	private string UserCode;
	private string Password;
	private string Url;
	public RealTimeTranscription(string URL,string UserCode,string Password,string Url)
	{
		this.URL = URL;
		this.UserCode = UserCode;
		this.Password = Password;
		this.Url = Url;
	}

	public async Task<string> GetTokenAsync()
	{
		using (var httpClient = new HttpClient())
		{
			var requestBody = new {
				userCode = UserCode,
				password = Password
			};

			string jsonRequestBody = System.Text.Json.JsonSerializer.Serialize(requestBody);
			using var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

			using var request = new HttpRequestMessage(HttpMethod.Post, Url) {
				Content = content
			};
			request.Headers.UserAgent.ParseAdd("Apifox/1.0.0 (https://apifox.com)");

			var response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			string responseBody = await response.Content.ReadAsStringAsync();

			var options = new JsonSerializerOptions {
				PropertyNameCaseInsensitive = true
			};
			try
			{
				var loginDto = System.Text.Json.JsonSerializer.Deserialize<LoginDto>(responseBody, options);
				Console.WriteLine("=========获取token成功，token：" + loginDto.Data.Token);
				return loginDto.Data.Token;
			} catch (System.Text.Json.JsonException e)
			{
				Console.WriteLine($"反序列化JSON时出错: {e.Message}");
				return null;
				// 进一步的异常处理逻辑
			}

		}
	}

	public async Task ConnectWebSocket()
	{
		client = new();
		Console.WriteLine("创建websocket");
		string token = await GetTokenAsync();
		try
		{
			// 添加access_token到头部信息
			client.Options.SetRequestHeader("Cookie", "access_token=" + token);
			// 连接WebSocket服务器
			Uri uri = new(URL);
			await client.ConnectAsync(uri, CancellationToken.None);
			Console.WriteLine("WebSocket connection opened.");
		} catch (Exception ex)
		{
			// 输出错误信息
			Console.WriteLine(ex.ToString());
		}
	}
	public void CloseWebSocket()
	{
		if(client != null)
		{
			try
			{
				client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
				client.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭连接", CancellationToken.None);
			}catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			
		}		
	}
	public async Task<List<string>> RealTimeTranscriptionAsync(string filepath)
	{	
		try
		{
			// 发送音频数据
			await SendDataAsync(filepath);
			// 接收WebSocket返回的数据
			return await ReceiveData();
		} catch (Exception ex)
		{
			// 输出错误信息
			Console.WriteLine(ex.ToString());
			return null;
		}
	}
	//发送文件
	public async Task SendDataAsync(string filepath)
	{
		if (client != null && client.State == WebSocketState.Open)
		{
			try
			{
				using (var fs = new FileStream(filepath, FileMode.Open, FileAccess.Read))
				{
					byte[] buffer = new byte[100 * 32];
					int bytesRead;

					// 每次读取1280个字节作为一个分片
					while ((bytesRead = await fs.ReadAsync(buffer,CancellationToken.None)) > 0)
					{
						// 在这里实现发送分片的逻辑，可以使用网络连接或其他方式发送字节数据
						await client.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);
					}
				}
			} catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
	//发送字节数组
	public async Task SendDataAsync(byte[] buffer)
	{
		if (client != null && client.State == WebSocketState.Open)
		{

			try
			{
				int bytesRead;
				using var ms = new MemoryStream(buffer);
				byte[] sendBuf = new byte[1280];
				// 每次读取1280个字节作为一个分片
				while ((bytesRead = await ms.ReadAsync(sendBuf, CancellationToken.None)) > 0)
				{
					// 在这里实现发送分片的逻辑，可以使用网络连接或其他方式发送字节数据
					await client.SendAsync(sendBuf, WebSocketMessageType.Binary, true, CancellationToken.None);
				}

			} catch (Exception e)
			{
				Console.WriteLine($"发送数据时发生异常：{e.Message}");
			}
		} else
		{
			Console.WriteLine("WebSocket未连接或已关闭。");
		}
	}

	public async Task<List<string>> ReceiveData()
	{
		var buffer = new byte[3200];
		List<string> preRes = new List<string>();
		List<string> res = new List<string>();
		while(client.State == WebSocketState.Open)
		{
			var result = await client.ReceiveAsync(buffer, CancellationToken.None);
			string jsonstring = Encoding.UTF8.GetString(buffer, 0, result.Count);
			AudioContent audiocontent = JsonConvert.DeserializeObject<AudioContent>(jsonstring);
			if (audiocontent != null)
			{

				string output = ParseAndConcatenate(jsonstring);
				if (!audiocontent.msgtype.Equals("sentence"))//一句话未结束
				{
					preRes.Add(output);
				} else
				{
					Console.WriteLine($"一句话结束 : {output}");
					res.Add(output);
				}
			}
		}
		if(res.Count == 0)
		{
			return preRes;
		} else
		{
			return res;
		}
	}

	public async Task ProcessWebSocketRequests(WebSocketReceiveResult receiveResult,WebSocket h5client)
	{
		byte[] buffer = new byte[100 * 32]; // 设置足够大的缓冲区来接收数据
		await ConnectWebSocket();

		while (client.State == WebSocketState.Open)
		{
			WebSocketReceiveResult result;
			result = await client.ReceiveAsync(buffer, CancellationToken.None);
			if(result.MessageType == WebSocketMessageType.Close)
			{
				await client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
				Console.WriteLine("client关闭连接");
				break;
			}
			await h5client.SendAsync(buffer,WebSocketMessageType.Binary, true, CancellationToken.None);
			//// 将接收到的字节片段转换为字符串
			//string jsonString = Encoding.UTF8.GetString(buffer, 0, result.Count);

			//// 反序列化JSON到自定义类型
			//AudioContent audioContent = JsonConvert.DeserializeObject<AudioContent>(jsonString);

			//if (audioContent != null)
			//{
			//	string output = ParseAndConcatenate(jsonString);
			//	if (!audioContent.msgtype.Equals("sentence"))//一句话未结束
			//	{
			//	} else
			//	{
			//		Console.WriteLine($"一句话结束 : {output}");
			//		byte[] sendBuffer = Encoding.UTF8.GetBytes(output);
			//		await h5client.SendAsync(new ArraySegment<byte>(sendBuffer, 0, sendBuffer.Length),
			//		result.MessageType, true, CancellationToken.None);
			//	}
			//} else
			//{
			//	Console.WriteLine("audio为空");
			//}
		}
	}

	public string ParseAndConcatenate(string jsonString)
	{
		if(string.IsNullOrEmpty(jsonString))
		{ return null; }
		StringBuilder result = new StringBuilder();
		try
		{
			using JsonDocument document = JsonDocument.Parse(jsonString);
			JsonElement root = document.RootElement;
			JsonElement.ArrayEnumerator wsArray = root.GetProperty("ws").EnumerateArray();

			while (wsArray.MoveNext())
			{
				JsonElement wsObj = wsArray.Current;
				JsonElement.ArrayEnumerator cwArray = wsObj.GetProperty("cw").EnumerateArray();
				while (cwArray.MoveNext())
				{
					string word = cwArray.Current.GetProperty("w").GetString();
					result.Append(word);
				}
			}
		} catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result.ToString();
	}
}