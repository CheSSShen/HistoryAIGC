/*
 * Copyright (C) Alibaba Cloud Computing
 * All rights reserved.
 * 
 */

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Aliyun.OSS.Common;
using System.Security.AccessControl;
using System.Collections.Generic;
using Aliyun.OSS;

namespace AIGCImageHistory
{
	/// <summary>
	/// Sample for getting object.
	/// </summary>
	public static class OSSRepository
	{
		static string accessKeyId;
		static string accessKeySecret;
		static string endpoint;
		static string fileToUpload;
		static OssClient client;
		private static List<Uri> list;
		static AutoResetEvent _event = new AutoResetEvent(false);
		public static string AccessKeyId { get => accessKeyId; set => accessKeyId = value; }
		public static string AccessKeySecret { get => accessKeySecret; set => accessKeySecret = value; }
		public static string Endpoint { get => endpoint; set => endpoint = value; }
		public static string FileToUpload { get => fileToUpload; set => fileToUpload = value; }
		public static OssClient Client { get => client; set => client = value; }

		public static List<Uri>? GetObject(string bucketName,string prefix)//获得OSS文件
		{
			list = new List<Uri>();
			list.Clear();
			try
			{
				ObjectListing result = null;
				string nextMarker = string.Empty;
				do
				{
					var listObjectsRequest = new ListObjectsRequest(bucketName) {
						Marker = nextMarker,
						MaxKeys = 100,
						Prefix = prefix,
						Delimiter = "/"
					};
					result = client.ListObjects(listObjectsRequest);
					foreach (var summary in result.ObjectSummaries)
					{
						if (summary.Key.Equals(prefix))
							continue;
						//Console.WriteLine(summary.Key);
						//keys.Add(summary.Key);
						list.Add(GetUri(bucketName, summary.Key));
					}
					nextMarker = result.NextMarker;
				} while (result.IsTruncated);
				Console.WriteLine("Get objects of bucket:{0} succeeded ", bucketName);
				return list;
			} catch (OssException ex)
			{
				Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
					ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
				return null;
			} catch (Exception ex)
			{
				Console.WriteLine("Failed with error info: {0}", ex.Message);
				return null;
			}
			
		}
		//public static void GetUris(string bucketName,OssObjectSummary summary)//获取uri
		//{
		//	try
		//	{
		//		var metadata = Client.GetObjectMetadata(bucketName, summary.Key);
		//		var etag = metadata.ETag;
		//		// 生成签名URL。
		//		var req = new GeneratePresignedUriRequest(bucketName, summary.Key, SignHttpMethod.Get) {
		//			// 设置签名URL过期时间，默认值为3600秒。
		//			Expiration = DateTime.Now.AddHours(1),
		//		};
		//		var uri = Client.GeneratePresignedUri(req);
		//		list.Add(uri);
		//	} catch (OssException ex)
		//	{
		//		Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
		//			ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
		//	} catch (Exception ex)
		//	{
		//		Console.WriteLine("Failed with error info: {0}", ex.Message);
		//	}
		//}
		public static Uri GetUri(string bucketName,string fileName)
		{
			return new Uri($"https://{bucketName}.oss-cn-hangzhou.aliyuncs.com/{fileName}");
		}

		public static bool PutObjectFromFile(string bucketName, string loadToPath)//上传
		{
			try
			{
				Client.PutObject(bucketName, loadToPath, FileToUpload);
				Console.WriteLine("Put object:{0} succeeded", loadToPath);
				return true;
			} catch (OssException ex)
			{
				Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
					ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
				return false;
			} catch (Exception ex)
			{
				Console.WriteLine("Failed with error info: {0}", ex.Message);
				return false;
			}
			
		}
		public static bool DeleteAllObject(string bucketName, string prefix)//删除prefix路径下所有文件
		{
			try
			{
				ObjectListing result = null;
				string nextMarker = string.Empty;
				do
				{
					var listObjectsRequest = new ListObjectsRequest(bucketName) {
						Marker = nextMarker,
						MaxKeys = 999,
						Prefix = prefix,
						Delimiter = "/"
					};
					result = client.ListObjects(listObjectsRequest);
					foreach (var summary in result.ObjectSummaries)
					{
						if (summary.Key.Equals(prefix))
							continue;
						if (!DeleteObject(bucketName, summary.Key))
						{
							throw new Exception("删除时出现异常");
						}
					}
					nextMarker = result.NextMarker;
				} while (result.IsTruncated);
				//Console.WriteLine("List objects of bucket:{0} succeeded ", bucketName);
				return true;
			} catch (OssException ex)
			{
				Console.WriteLine("Failed with error code: {0}; Error info: {1}. \nRequestID:{2}\tHostID:{3}",
					ex.ErrorCode, ex.Message, ex.RequestId, ex.HostId);
				return false;
			} catch (Exception ex)
			{
				Console.WriteLine("Failed with error info: {0}", ex.Message);
				return false;
			}

		}
		public static bool DeleteObject(string bucketName, string objectName)//删除文件
		{
			try
			{
				// 删除文件。
				client.DeleteObject(bucketName, objectName);
				Console.WriteLine("Delete object succeeded");
				return true;
			} catch (Exception ex)
			{

				Console.WriteLine("Delete object failed. {0}", ex.Message);
				return false;
			}
		}
	}
}