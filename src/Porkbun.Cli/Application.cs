using Microsoft.Extensions.Options;
using Porkbun.Cli.Models;
using Porkbun.Cli.Services;
using Porkbun.Cli.Settings;
using System.Text.Json;

namespace Porkbun.Cli
{
    public class Application
	{
		private readonly ILogger<Application> Logger;
		private readonly IPorkbunClient PorkbunClient;
		private readonly IHostEnvironment HostEnvironment;
		private readonly ApplicationSettings Settings;
		private readonly IReadOnlyList<Domain> Domains;

		public Application(
			ILogger<Application> logger,
			IPorkbunClient porkbunClient,
			IHostEnvironment hostEnvironment,
			IOptionsMonitor<ApplicationSettings> applicationOptions,
			IOptionsMonitor<DomainSettings> domainOptions)
		{
			Logger = logger;
			PorkbunClient = porkbunClient;
			HostEnvironment = hostEnvironment;
			Settings = applicationOptions.CurrentValue;
			Domains = domainOptions.CurrentValue.Domains;
		}

		public async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			if (!Domains.Any())
			{
				Logger.LogWarning("Empty domain file");
				return;
			}

			var shouldSave = false;
			var pingFilePath = Settings.PingFile;
			if (!Path.IsPathFullyQualified(Settings.PingFile))
			{
				pingFilePath = Path.Join(HostEnvironment.ContentRootPath, Settings.PingFile);
			}

			if (!File.Exists(pingFilePath))
			{
				using var _ = File.Create(pingFilePath);
			}

			var pingFile = new PingFile();

			var pingFileContents = await File.ReadAllTextAsync(pingFilePath, cancellationToken);
			if (!string.IsNullOrWhiteSpace(pingFileContents))
			{
				pingFile = JsonSerializer.Deserialize<PingFile?>(pingFileContents);

				Logger.LogInformation("Ping file found! {ipAddress}", pingFile!.IpAddress);
			}
			
			if (pingFile?.DateLastModified.AddSeconds(pingFile?.PingCacheTtl ?? 900) <= DateTimeOffset.UtcNow)
			{
				Logger.LogInformation("Re-pinging...");
				var pingResponse = await PorkbunClient.PingAsync(cancellationToken);

				Logger.LogInformation("Ping Complete! {ipAddress}", pingResponse.YourIp);

				pingFile.DateLastModified = DateTimeOffset.UtcNow;
				pingFile.IpAddress = pingResponse.YourIp.ToString();
				pingFile.PingCacheTtl = Settings.PingCacheTtl;

				shouldSave = true;
			}

			if (shouldSave)
			{
				Logger.LogInformation("Saving ping file!");
				pingFileContents = JsonSerializer.Serialize(pingFile);
				await File.WriteAllTextAsync(pingFilePath, pingFileContents, cancellationToken);
			}

			foreach (var domain in Domains)
			{
				Logger.LogInformation($"{domain.Name} - {domain.Subdomains.Count} subdomains");

				foreach (var subdomain in domain.Subdomains)
				{

					Logger.LogInformation($"{subdomain.Key} - {subdomain.Value.Count} dns entries");
					
					foreach (var dnsEntry in subdomain.Value)
					{
						var content = ReplaceForPingIp(dnsEntry.Value, pingFile!.IpAddress.ToString());
						Logger.LogInformation($"Updating {subdomain.Key}.{domain.Name} - {dnsEntry.Key}:{content}");
						var result = await PorkbunClient.UpdateSubdomainAsync(domain.Name, subdomain.Key, dnsEntry.Key, content, cancellationToken);

						if (!result.Success)
						{
							Logger.LogError(result.Message);
							return;
						}
					}

				}
			}
		}

		private string ReplaceForPingIp(string value, string pingIp)
		{
			return value.Replace("$PING_IP", pingIp);
		}
	}
}