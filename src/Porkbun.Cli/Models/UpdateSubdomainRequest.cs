using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Porkbun.Cli.Models
{
	public class UpdateSubdomainRequest : SecureRequest
	{
		public string Content { get; init; }
		public int? Ttl { get; init; }
	}
}
