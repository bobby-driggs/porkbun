using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Porkbun.Cli.Models
{
	public class UpdateByIdRequest : SecureRequest
	{
		/// <summary>
		/// Name of the subdomain: www, etc
		/// </summary>
		public string Name { get; init; }

		/// <summary>
		/// The type of record: A, AAAA, etc
		/// </summary>
		public string Type { get; init; }

		/// <summary>
		/// The IP Address
		/// </summary>
		public string Content { get; init; }

		public int? Ttl { get; init; }
	}
}
