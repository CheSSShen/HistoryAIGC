namespace AIGCImageHistory
{
	public static class AIGCConfigurationExtensions
	{
		public static WebApplicationBuilder AddAIGCConfiguration(this WebApplicationBuilder builder
																,string path = "appsettings.json")
		{
			ConfigurationBuilder builderConfig = new ConfigurationBuilder();
			builderConfig.AddJsonFile(path, optional: false, reloadOnChange: true);
			IConfigurationRoot configuration = builder.Configuration;
			builder.Services.AddOptions().Configure<AIGCConfig>(e => configuration.Bind(e));
			return builder;
		}
	}
}
