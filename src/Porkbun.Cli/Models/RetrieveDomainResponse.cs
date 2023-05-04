using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Porkbun.Cli.Models
{
    public class RetrieveDomainResponse : ResponseBase
    {
        public DnsRecord[]? Records { get; set; }
    }

    public class DnsRecord
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public string? Content { get; set; }
        public string? Ttl { get; set; }
        public string? Prio { get; set; }
        public string? Notes { get; set; }
    }
}
