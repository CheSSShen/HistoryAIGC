namespace AIGCImageHistory.RealTimeTranscript
{

	public class AudioContent
	{
		public int segId { get; set; }
		public int bg { get; set; }
		public int ed { get; set; }
		public int ei { get; set; }
		public bool ls { get; set; }
		public string metadata { get; set; }
		public string msgtype { get; set; }
		public int sn { get; set; }
		public int pa { get; set; }
		public Vad vad { get; set; }
		public W1[] ws { get; set; }
		public Speakeritem speakerItem { get; set; }
		public Namemapping nameMapping { get; set; }
	}

	public class Vad
	{
		public int vadBegin { get; set; }
		public int vadEnd { get; set; }
		public W[] ws { get; set; }
	}

	public class W
	{
		public int bg { get; set; }
		public int ed { get; set; }
	}

	public class Speakeritem
	{
		public float score { get; set; }
		public string value { get; set; }
	}

	public class Namemapping
	{
	}

	public class W1
	{
		public int bg { get; set; }
		public Cw[] cw { get; set; }
	}

	public class Cw
	{
		public int rl { get; set; }
		public string sc { get; set; }
		public int sf { get; set; }
		public string w { get; set; }
		public int wb { get; set; }
		public string wc { get; set; }
		public int we { get; set; }
		public string wp { get; set; }
	}

}
