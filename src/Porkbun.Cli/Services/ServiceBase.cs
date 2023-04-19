using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Services
{
	public class ServiceBase
	{
		protected readonly IHttpClientFactory HttpClientFactory;
		protected readonly ILogger Logger;
		protected readonly string ServiceClassName;

		public ServiceBase(IHttpClientFactory httpClientFactory, ILogger logger)
		{
			HttpClientFactory = httpClientFactory;
			Logger = logger;
			ServiceClassName = GetType().Name;
		}

		public async Task<TResult> ProcessRequestAsync<TResult>(Func<HttpClient, Task<HttpResponseMessage>> action, Func<string, TResult> reduce, [CallerMemberName] string memberName = "")
		{
			using var logScope = Logger.BeginScope(new { ServiceName = ServiceClassName, Method = memberName });

			var httpClient = HttpClientFactory.CreateClient(ServiceClassName);

			try
			{
				var response = await action(httpClient);

				if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.BadRequest)
				{
					response.EnsureSuccessStatusCode();
				}

				var content = await response.Content.ReadAsStringAsync();

				return reduce(content);
			}
			catch (Exception e)
			{
				Logger.LogError(e, "Error while processing HTTP Call");
				throw;
			}
			finally
			{

			}
		}
	}
}
