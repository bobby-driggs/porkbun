using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Settings
{
    public class ApplicationSettings
    {
        public string? PingFile { get; init; }
        public int PingCacheTtl { get; init; }
    }
}
