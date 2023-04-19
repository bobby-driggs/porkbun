using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Models
{
	public class PingFile
	{
		public string IpAddress { get; set; }
		public int PingCacheTtl { get; set; }
		public DateTimeOffset DateLastModified { get; set; }
	}
}
