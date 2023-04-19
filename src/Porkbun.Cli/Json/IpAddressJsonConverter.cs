using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;

namespace Porkbun.Cli.Json
{
	public class IpAddressJsonConverter : JsonConverter<IPAddress>
	{
		public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return IPAddress.Parse(reader.GetString()!);
		}

		public override void Write(Utf8JsonWriter writer, IPAddress ipAddressValue, JsonSerializerOptions options)
		{
			writer.WriteStringValue(ipAddressValue.ToString());
		}
	}
}
