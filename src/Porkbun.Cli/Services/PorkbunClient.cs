using Microsoft.Extensions.Options;
using Porkbun.Cli.Models;
using Porkbun.Cli.Settings;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace Porkbun.Cli.Services
{
	public interface IPorkbunClient
	{
		Task<PingResponse> PingAsync(CancellationToken cancellationToken);
		Task<ResponseBase> UpdateSubdomainAsync(string domain, string subdomain, string recordType, string content, CancellationToken cancellationToken);
	}

	public class PorkbunClient : ServiceBase, IPorkbunClient
	{
		private readonly JsonSerializerOptions JsonSerializerOptions;
		private readonly PorkbunSettings Settings;
		public PorkbunClient(IHttpClientFactory httpClientFactory, ILogger<PorkbunClient> logger, IOptionsMonitor<PorkbunSettings> options)
			: base(httpClientFactory, logger)
		{
			JsonSerializerOptions = new JsonSerializerOptions()
			{
				PropertyNameCaseInsensitive = true,
				NumberHandling = JsonNumberHandling.AllowReadingFromString,
			};
			Settings = options.CurrentValue;
		}

		public async Task<PingResponse> PingAsync(CancellationToken cancellationToken)
		{
			var payload = new SecureRequest
			{
				ApiKey = Settings.ApiKey!,
				ApiSecret = Settings.ApiSecret!,
			};

			var uri = new Uri(Settings.ApiUri!, "ping");
			var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
			{
				Content = JsonContent.Create(payload)
			};

			var response = await ProcessRequestAsync(
				async (httpClient) =>
				{
					return await httpClient.SendAsync(httpRequest, cancellationToken);
				}, (content) =>
				{
					return JsonSerializer.Deserialize<PingResponse>(content, JsonSerializerOptions);
				});

			return response!;
		}

		public async Task<ResponseBase> UpdateSubdomainAsync(string domain, string subdomain, string recordType, string content, CancellationToken cancellationToken)
		{
			var payload = new UpdateSubdomainRequest
			{
				ApiKey = Settings.ApiKey!,
				ApiSecret = Settings.ApiSecret!,
				Content = content,
				Ttl = 900,
			};

			var uri = new Uri(Settings.ApiUri!, $"dns/editByNameType/{domain}/{recordType}/{subdomain}");
			var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
			{
				Content = JsonContent.Create(payload)
			};

			var response = await ProcessRequestAsync(
				async (httpClient) =>
				{
					return await httpClient.SendAsync(httpRequest, cancellationToken);
				}, (content) =>
				{
					return JsonSerializer.Deserialize<ResponseBase>(content, JsonSerializerOptions);
				});

			return response!;
		}
	}
}
