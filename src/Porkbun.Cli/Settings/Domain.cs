using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Settings
{
	public class Domain
	{
		public string Name { get; init; }
		public Dictionary<string, Dictionary<string, string>> Subdomains { get; init; }
	}
}
