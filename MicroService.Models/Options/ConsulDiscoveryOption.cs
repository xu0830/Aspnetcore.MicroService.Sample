using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.Options
{
    public class ConsulDiscoveryOption
    {
        public string? Schema { get; set; }
        public string? ServiceName { get; set; }
        public string? Hostname { get; set; }
        public int Port { get; set; }
        public string? InstanceId { get; set; }
        public string[]? Tags { get; set; }
        public string? HealthCheckUrl { get; set; }
        public RetryOption? RetryOption { get; set; }

    }
}
