using Porkbun.Cli;
using Porkbun.Cli.Services;
using Porkbun.Cli.Settings;

ILogger<Program>? Logger = null;
var cancellationTokenSource = new CancellationTokenSource();

Console.CancelKeyPress += Console_CancelKeyPress;

var host = Host.CreateDefaultBuilder(args)
	.ConfigureAppConfiguration(builder =>
	{
		builder.AddYamlFile("domains.yaml");
	})
	.ConfigureServices((context, services) =>
	{
		services.AddHttpClient();
		services.AddOptions();
		services.Configure<PorkbunSettings>((settings) =>
		{
			context.Configuration.GetSection("PorkbunSettings")
				.Bind(settings);
		});
		services.Configure<ApplicationSettings>((settings) =>
		{
			context.Configuration.GetSection("ApplicationSettings")
				.Bind(settings);
		});
		services.Configure<DomainSettings>((settings) =>
		{
			context.Configuration
				.Bind(settings);
		});
		services.AddTransient<IPorkbunClient, PorkbunClient>();
		services.AddSingleton<Application>();
	})
	.Build();

var app = host.Services.GetRequiredService<Application>();
Logger = host.Services.GetRequiredService<ILogger<Program>>();

Logger.LogDebug("Application run");

await app.ExecuteAsync(cancellationTokenSource.Token);

Logger.LogDebug("Application complete!");

void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
{
	Logger?.LogDebug("Cancel requested!");
	cancellationTokenSource.Cancel();
}