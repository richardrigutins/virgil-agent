namespace VirgilAgent.BotService;

public class Program
{
	public static void Main(string[] args)
	{
		CreateHostBuilder(args).Build().Run();
	}

	public static IHostBuilder CreateHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.ConfigureLogging((logging) =>
				{
					logging.AddDebug();
					logging.AddConsole();
				});
				webBuilder.UseStartup<Startup>();
			});
}

