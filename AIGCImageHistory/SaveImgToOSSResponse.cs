namespace AIGCImageHistory
{
	public class SaveImgToOSSResponse
	{
		public int? StatusCode { get; set; }
		public Object? Data { get; set; }
		public string? Message {  get; set; }
		public SaveImgToOSSResponse(Object data) 
		{
			Data = data;	
		}
		public SaveImgToOSSResponse() { }

	}
}
