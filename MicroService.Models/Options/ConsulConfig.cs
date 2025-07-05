using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.Options
{
    /// <summary>
    /// consul 服务注册配置
    /// </summary>
    public class ConsulConfig
    {
        public string? Schema { get; set; } = "http";
        public string? Host { get; set; }

        public int Port { get; set; }
        public ConsulDiscoveryOption? Discovery { get; set; }
    }
}
