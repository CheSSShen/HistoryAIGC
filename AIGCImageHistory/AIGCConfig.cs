namespace AIGCImageHistory
{
	public class AIGCConfig
	{
		public string bucketName { get; set; }
		public string prefix { get; set; }
		public string xossprocessstyle { get; set; }
		public string FileToUpload { get; set; }
		public string Urls { get; set; }
		public string ServerFolderPath {  get; set; }
		public SFTPServer SFTPServer { get; set; }
	}
	public class SFTPServer
	{
		public string host { get; set; }
		public int port { get; set; }
		public string username { get; set; }
		public string password { get; set; }
	}
}
