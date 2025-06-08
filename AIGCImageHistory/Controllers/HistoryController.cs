using Aliyun.OSS;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System.Buffers.Text;
using System.IO;
using System.Net;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Text;

namespace AIGCImageHistory.Controllers
{
	[Route("[action]")]
	[ApiController]
	public class HistoryController : ControllerBase
	{
		private readonly IWebHostEnvironment _hostingEnvironment;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly OssClient _ossClient;
		private readonly IOptionsSnapshot<AIGCConfig> _config;
		private readonly IConfiguration _configuration;
		public HistoryController(IHttpClientFactory httpClientFactory, OssClient ossClient, IOptionsSnapshot<AIGCConfig> config, IWebHostEnvironment hostingEnvironment, IConfiguration configuration)
		{
			_httpClientFactory = httpClientFactory;
			_ossClient = ossClient;
			_config = config;
			_hostingEnvironment = hostingEnvironment;
			_configuration = configuration;
		}

		[HttpPost]
		public ActionResult SaveImgToOSS(SaveImgToOSSRequest request)//保存对应client生成的图片
		{
			SaveImgToOSSResponse response = new SaveImgToOSSResponse();
			//通过FTP获取算力服务器上的图片
			bool b = GetImageHelper.GetImage(_config,request.FileNames);
			if (!b)
			{
				response.Message = "通过FTP获取图片失败";
				return Ok(response);
			}
			List<Uri> imgUris = new List<Uri>();
			OSSRepository.Client = _ossClient;
			string style = _config.Value.xossprocessstyle;
			string bucketName = _config.Value.bucketName;
			string prefix = _config.Value.prefix + $"clientId_{request.ClientId}/";
			for (int i = 0; i < request.FileNames.Length; i++)
			{
				string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
				OSSRepository.FileToUpload = _config.Value.FileToUpload + "/" + request.FileNames[i];
				string uploadname = prefix + "Image_" + timestamp + $"_{i+1}";
				if (!OSSRepository.PutObjectFromFile(bucketName, uploadname))
				{
					response.StatusCode = 500;
					response.Message = $"上传第{i + 1}张图片失败";
					return Ok(response);
				}

				if (OSSRepository.GetObject(bucketName, prefix).Count == 0)
				{
					response.StatusCode = 500;
					response.Message = $"获取图片失败";
					return Ok(response);
				} else
				{
					Uri currentUri = OSSRepository.GetUri(bucketName, uploadname);
					imgUris.Add(new Uri(currentUri.ToString()
						.Split("?Expires")[0] + $"?x-oss-process=style/{style}"));
				}
			}
			response.Data = imgUris;
			response.StatusCode = 200;
			return Ok(response);
		}

		[HttpGet]
		public ActionResult<Response> GetHistoryImg(string clientId)//获取clientId的历史图片
		{
			string style = _config.Value.xossprocessstyle;
			List<Uri> uris = new List<Uri>();
			OSSRepository.Client = _ossClient;
			uris = OSSRepository.GetObject(_config.Value.bucketName,
												_config.Value.prefix + $"clientId_{clientId}/");
			if(uris == null || uris.Count == 0)
			{
				return Ok(new Response() { Code = 404, imgUris = uris, message = "uris为空或者没有此clientId" });
			} else
			{
				for (int i = 0; i < uris.Count; i++)
				{
					uris[i] = new Uri(uris[i].ToString().Split("?Expires")[0] + $"?x-oss-process=style/{style}");
					Console.WriteLine(uris[i]);
				}
				return Ok(new Response() { Code = 200, imgUris = uris });
			}
		}
		[HttpDelete]
		public ActionResult<Response> ClearHistoryImg(string clientId)
		{
			OSSRepository.Client = _ossClient;
			string prefix = _config.Value.prefix + $"clientId_{clientId}/";
			bool r = OSSRepository.DeleteAllObject(_config.Value.bucketName, prefix);
            if (r)
            {
				return Ok(new Response() { Code = 200, message = "清除成功" });
			} else
			{
				return Ok(new Response() { Code = 400, message = "清除失败" });
			}
        }

		[HttpGet]
		public async Task<ActionResult<string>> GetProgress(string clientId)
		{
			CheckProgressHelper checkProgressHelper = new CheckProgressHelper();
			await checkProgressHelper.ConnectAsync("ws://10.1.251.11:8188", clientId);
			return Ok(checkProgressHelper.ReceiveMessageAsync());
		}

		[HttpPost]
		public async Task<ActionResult> RealTimeTranscript(IFormFile file)
		{
			if (file == null || file.Length == 0)
			{
				return null;
			}

			// 构造文件保存路径
			string uploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "audios");
			Directory.CreateDirectory(uploadsFolder); // 如果路径不存在，创建路径

			// 为了避免文件名冲突，使用唯一的文件名
			string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
			string filePath = Path.Combine(uploadsFolder, uniqueFileName);

			// 保存文件
			using (var fileStream = new FileStream(filePath, FileMode.Create))
			{
				await file.CopyToAsync(fileStream);
			}
			try
			{
				RealTimeTranscription realTimeTranscription = new RealTimeTranscription(
						_configuration.GetSection("realtimeWebsocketURL").Value!,
						_configuration.GetSection("realtimeUserCode").Value!,
						_configuration.GetSection("realtimePassword").Value!,
						_configuration.GetSection("realtimeUrl").Value!
						);
				await realTimeTranscription.ConnectWebSocket();
				var result = await realTimeTranscription.RealTimeTranscriptionAsync(filePath);
				if (result == null)
				{
					return BadRequest("转换失败，结果为空");
				}
				return Ok(result);
			} catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

		[Route("/ws")]
		[HttpGet]
		public async Task Get()
		{
			if (HttpContext.WebSockets.IsWebSocketRequest)
			{
				using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
				RealTimeTranscription realTimeTranscription = new RealTimeTranscription(
				_configuration.GetSection("realtimeWebsocketURL").Value!,
				_configuration.GetSection("realtimeUserCode").Value!,
				_configuration.GetSection("realtimePassword").Value!,
				_configuration.GetSection("realtimeUrl").Value!
				);
				// 创建缓冲区
				//var buffer = new byte[1024 * 400];

				while (webSocket.State == WebSocketState.Open)
				{
					var buffer = new ArraySegment<byte>(new byte[409600]);
					byte[] decodedBytes = null;
					// 接收来自客户端的数据
					WebSocketReceiveResult receivedResult;
					WebSocket targetWebSocket = null;
					using (var ms = new MemoryStream())
					{
						do
						{
							receivedResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
							ms.Write(buffer.Array, buffer.Offset, receivedResult.Count);
							Console.WriteLine($"-------收到前端的数据:{Encoding.UTF8.GetString(buffer)}-------");
							if (realTimeTranscription.client == null)
							{
								await realTimeTranscription.ConnectWebSocket();
								targetWebSocket = realTimeTranscription.client;
								Console.WriteLine("-------连接实时转写服务器成功-------");
							}
						} while (!receivedResult.EndOfMessage);

						ms.Seek(0, SeekOrigin.Begin);
						if (receivedResult.MessageType == WebSocketMessageType.Text)
						{
							byte[] receivedBytes = ms.ToArray();
							var base64EncodedString = Encoding.UTF8.GetString(receivedBytes);
							while (base64EncodedString.Length % 4 != 0)
							{
								base64EncodedString += "=";
							}
							try
							{
								decodedBytes = Convert.FromBase64String(base64EncodedString);
							} catch (Exception ex)
							{
								Console.WriteLine("base64转成byte数组失败:  " + ex.Message);
							}

						}
					}



					//System.IO.File.WriteAllBytes("D:\\desktop\\audio.wav", buffer);
					//if (receivedResult.MessageType == WebSocketMessageType.Close)
					//{
					//	// 客户端请求关闭连接
					//	try
					//	{
					//		await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
					//		await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭连接", CancellationToken.None);
					//		Console.WriteLine("-------关闭和前端的websocket连接-------");
					//		realTimeTranscription.CloseWebSocket();
					//		Console.WriteLine("-------关闭和服务端的websocket连接-------");
					//	} catch (Exception ex)
					//	{
					//		Console.WriteLine(ex.Message);
					//	}
					//	break;
					//}

					// 将接收到的数据透传到目标服务器
					if (decodedBytes == null)
						Console.WriteLine("decodedBytes为空");
					await realTimeTranscription.SendDataAsync(decodedBytes);
					var readBuffer = new byte[100 * 32];
					// 读取来自目标服务器的响应

					WebSocketReceiveResult targetReceivedResult;
					do
					{
						targetReceivedResult = await targetWebSocket.ReceiveAsync(new ArraySegment<byte>(readBuffer), CancellationToken.None);
						Console.WriteLine("-------响应数据 : " + Encoding.UTF8.GetString(readBuffer) + readBuffer.Length);
						if (targetReceivedResult.MessageType == WebSocketMessageType.Close)
						{
							try
							{
								realTimeTranscription.CloseWebSocket();
								Console.WriteLine("-------关闭和服务端的websocket连接-------");
								await webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
								await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "关闭连接", CancellationToken.None);
								Console.WriteLine("-------关闭和前端的websocket连接-------");
							} catch (Exception ex)
							{
								Console.WriteLine(ex.Message);
							}
							break;
						}
						// 将目标服务器的响应数据透传回客户端
						await webSocket.SendAsync(new ArraySegment<byte>(readBuffer, 0, targetReceivedResult.Count),
												  targetReceivedResult.MessageType,
												  targetReceivedResult.EndOfMessage,
												  CancellationToken.None);
						//Console.WriteLine("-------传回前端的数据 : " + Encoding.UTF8.GetString(readBuffer));
					} while (targetReceivedResult.MessageType != WebSocketMessageType.Close);
				}
			} else
			{
				HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
			}
		}
	}
}
