﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Settings
{
	public class Domain
	{
		public string Name { get; init; }
		public List<Subdomain> Subdomains { get; init; }
	}
}
