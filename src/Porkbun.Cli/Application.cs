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
            var pingFilePath = Settings.PingFile!;
            if (!Path.IsPathFullyQualified(Settings.PingFile!))
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
                pingFile.IpAddress = pingResponse.YourIp!.ToString();
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

                var existingRecords = await PorkbunClient.GetDnsRecordsForDomainsAsync(domain.Name, cancellationToken);

                if (!existingRecords.Success)
                {
                    Logger.LogError($"Failed to get DNS Recorsd for domain: {domain.Name}. Skipping");
                    continue;
                }

                foreach (var subdomain in domain.Subdomains)
                {
                    Logger.LogInformation($"Processing {subdomain.Name}");

                    var fullname = subdomain.Name == string.Empty
                        ? domain.Name
                        : $"{subdomain.Name}.{domain.Name}";

                    var content = ReplaceForPingIp(subdomain.Content, pingFile!.IpAddress.ToString());

                    var existingRecord = existingRecords.Records.FirstOrDefault(uu => uu.Name == fullname && uu.Type == subdomain.Type);
                    if (existingRecord != null)
                    {
                        Logger.LogInformation($"Updating {fullname}'s {subdomain.Type} record to {content} via Id: {existingRecord.Id}");
                        var updateResult = await PorkbunClient.UpdateRecordByIdAsync(domain.Name, existingRecord.Id, subdomain.Name, subdomain.Type, content, cancellationToken);
                        if (!updateResult.Success)
                        {
                            Logger.LogWarning($"Enable to update {fullname}. Reason: {updateResult.Message}");
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