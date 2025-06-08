using Microsoft.Extensions.Options;
using Renci.SshNet;
using System;
using System.IO;

namespace AIGCImageHistory
{
	public static class GetImageHelper
	{
		public static bool GetImage(IOptionsSnapshot<AIGCConfig> config,string[] FileNames)
		{
			if (!Directory.Exists(config.Value.FileToUpload))
			{
				Directory.CreateDirectory(config.Value.FileToUpload);
				Console.WriteLine("目录已创建.");
			} else
			{
				Console.WriteLine("目录已经存在.");
			}

			// SFTP服务器信息
			var host = config.Value.SFTPServer.host;
			var port = config.Value.SFTPServer.port; // SFTP端口
			var username = config.Value.SFTPServer.username;
			var password = config.Value.SFTPServer.password; // 应使用更安全的方式处理密码

			using (var sftp = new SftpClient(host, port, username, password))
			{
				try
				{
					sftp.Connect();
					Console.WriteLine("连接到SFTP服务器成功.");

					var files = sftp.ListDirectory(config.Value.ServerFolderPath);
					foreach (var file in files)
					{
						if (file.IsRegularFile && FileNames.Contains(file.Name))
						{
							string localFilePath = Path.Combine(config.Value.FileToUpload, file.Name);
							using (var fileStream = File.OpenWrite(localFilePath))
							{
								sftp.DownloadFile(file.FullName, fileStream);
							}
						}
					}

					Console.WriteLine("文件下载完成.");
				} catch (Exception ex)
				{
					Console.WriteLine("发生错误: " + ex.Message);
					return false;
				} finally
				{
					if (sftp.IsConnected)
					{
						sftp.Disconnect();
						Console.WriteLine("从SFTP服务器断开.");
					}
				}
				return true;
			}
		}
	}
}