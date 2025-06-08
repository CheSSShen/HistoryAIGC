
using Aliyun.OSS;
using Microsoft.Extensions.Configuration;

namespace AIGCImageHistory
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			string myPloicy = "MyPolicy";
			builder.Services.AddCors(options => {
				options.AddPolicy(name: myPloicy, policy => {
					policy.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod();
				});
			});
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
			builder.Services.AddHttpClient();
			string accessKeyId = builder.Configuration.GetSection("AccessKey").GetValue<string>("accessKeyId")!;
			string accessKeySecret = builder.Configuration.GetSection("AccessKey").GetValue<string>("accessKeySecret")!;
			string endpoint = builder.Configuration.GetSection("AccessKey").GetValue<string>("endpoint")!;
			builder.Services.AddSingleton<OssClient>(new OssClient(endpoint, accessKeyId, accessKeySecret));
			builder.AddAIGCConfiguration();
			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}
			var webSocketOptions = new WebSocketOptions {
				KeepAliveInterval = TimeSpan.FromMinutes(1)
			};

			app.UseWebSockets(webSocketOptions);
			app.UseCors(myPloicy);
			app.UseAuthorization();

			app.MapControllers();
			app.Run();
		}
	}
}
