using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Models
{
	public class ResponseBase
	{
		public string? Status { get; init; }
		public string? Message { get; init; }

		public bool Success => Status == "SUCCESS";
	}
}
