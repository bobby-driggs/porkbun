using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Porkbun.Cli.Models
{
	public class SecureRequest
	{
		[JsonPropertyName("apikey")]
		public string? ApiKey { get; init; }

		[JsonPropertyName("secretapikey")]
		public string? ApiSecret { get; init; }
	}
}
