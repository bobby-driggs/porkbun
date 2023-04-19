using Porkbun.Cli.Json;
using System.Net;
using System.Text.Json.Serialization;

namespace Porkbun.Cli.Models
{
	public class PingResponse : ResponseBase
	{
		[JsonConverter(typeof(IpAddressJsonConverter))]
		public IPAddress? YourIp { get; set; }
	}
}
