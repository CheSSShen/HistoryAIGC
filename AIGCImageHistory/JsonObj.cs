namespace AIGCImageHistory
{

	public class Rootobject
	{
		public string client_id { get; set; }
		public Prompt prompt { get; set; }
	}

	public class Prompt
	{
		public _3 _3 { get; set; }
		public _4 _4 { get; set; }
		public _5 _5 { get; set; }
		public _8 _8 { get; set; }
		public _11 _11 { get; set; }
		public _13 _13 { get; set; }
		public _16 _16 { get; set; }
		public _38 _38 { get; set; }
		public _39 _39 { get; set; }
		public _40 _40 { get; set; }
		public _60 _60 { get; set; }
		public _67 _67 { get; set; }
		public _69 _69 { get; set; }
		public _74 _74 { get; set; }
		public _76 _76 { get; set; }
	}

	public class _3
	{
		public Inputs inputs { get; set; }
		public string class_type { get; set; }
		public _Meta _meta { get; set; }
	}

	public class Inputs
	{
		public long seed { get; set; }
		public int steps { get; set; }
		public int cfg { get; set; }
		public string sampler_name { get; set; }
		public string scheduler { get; set; }
		public int denoise { get; set; }
		public object[] model { get; set; }
		public object[] positive { get; set; }
		public object[] negative { get; set; }
		public object[] latent_image { get; set; }
	}

	public class _Meta
	{
		public string title { get; set; }
	}

	public class _4
	{
		public Inputs1 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta1 _meta { get; set; }
	}

	public class Inputs1
	{
		public string ckpt_name { get; set; }
	}

	public class _Meta1
	{
		public string title { get; set; }
	}

	public class _5
	{
		public Inputs2 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta2 _meta { get; set; }
	}

	public class Inputs2
	{
		public int width { get; set; }
		public int height { get; set; }
		public int batch_size { get; set; }
	}

	public class _Meta2
	{
		public string title { get; set; }
	}

	public class _8
	{
		public Inputs3 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta3 _meta { get; set; }
	}

	public class Inputs3
	{
		public object[] samples { get; set; }
		public object[] vae { get; set; }
	}

	public class _Meta3
	{
		public string title { get; set; }
	}

	public class _11
	{
		public Inputs4 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta4 _meta { get; set; }
	}

	public class Inputs4
	{
		public string instantid_file { get; set; }
	}

	public class _Meta4
	{
		public string title { get; set; }
	}

	public class _13
	{
		public Inputs5 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta5 _meta { get; set; }
	}

	public class Inputs5
	{
		public string image { get; set; }
		public string upload { get; set; }
	}

	public class _Meta5
	{
		public string title { get; set; }
	}

	public class _16
	{
		public Inputs6 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta6 _meta { get; set; }
	}

	public class Inputs6
	{
		public string control_net_name { get; set; }
	}

	public class _Meta6
	{
		public string title { get; set; }
	}

	public class _38
	{
		public Inputs7 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta7 _meta { get; set; }
	}

	public class Inputs7
	{
		public string provider { get; set; }
	}

	public class _Meta7
	{
		public string title { get; set; }
	}

	public class _39
	{
		public Inputs8 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta8 _meta { get; set; }
	}

	public class Inputs8
	{
		public string text { get; set; }
		public object[] clip { get; set; }
	}

	public class _Meta8
	{
		public string title { get; set; }
	}

	public class _40
	{
		public Inputs9 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta9 _meta { get; set; }
	}

	public class Inputs9
	{
		public object[] text { get; set; }
		public object[] clip { get; set; }
	}

	public class _Meta9
	{
		public string title { get; set; }
	}

	public class _60
	{
		public Inputs10 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta10 _meta { get; set; }
	}

	public class Inputs10
	{
		public float weight { get; set; }
		public int start_at { get; set; }
		public int end_at { get; set; }
		public object[] instantid { get; set; }
		public object[] insightface { get; set; }
		public object[] control_net { get; set; }
		public object[] image { get; set; }
		public object[] model { get; set; }
		public object[] positive { get; set; }
		public object[] negative { get; set; }
		public object[] image_kps { get; set; }
	}

	public class _Meta10
	{
		public string title { get; set; }
	}

	public class _67
	{
		public Inputs11 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta11 _meta { get; set; }
	}

	public class Inputs11
	{
		public string image { get; set; }
		public string upload { get; set; }
	}

	public class _Meta11
	{
		public string title { get; set; }
	}

	public class _69
	{
		public Inputs12 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta12 _meta { get; set; }
	}

	public class Inputs12
	{
		public string embedding { get; set; }
		public int emphasis { get; set; }
		public bool append { get; set; }
		public string text { get; set; }
	}

	public class _Meta12
	{
		public string title { get; set; }
	}

	public class _74
	{
		public Inputs13 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta13 _meta { get; set; }
	}

	public class Inputs13
	{
		public object[] pixels { get; set; }
		public object[] vae { get; set; }
	}

	public class _Meta13
	{
		public string title { get; set; }
	}

	public class _76
	{
		public Inputs14 inputs { get; set; }
		public string class_type { get; set; }
		public _Meta14 _meta { get; set; }
	}

	public class Inputs14
	{
		public string filename_prefix { get; set; }
		public object[] images { get; set; }
	}

	public class _Meta14
	{
		public string title { get; set; }
	}

}


