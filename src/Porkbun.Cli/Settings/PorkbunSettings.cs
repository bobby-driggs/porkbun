using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Settings
{
    public class PorkbunSettings
    {
        public string? ApiKey { get; init; }
        public string? ApiSecret { get; init; }
        public Uri? ApiUri { get; init; }
    }
}
