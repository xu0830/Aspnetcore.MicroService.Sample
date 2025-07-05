using Consul;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Handler
{
    internal class ConsulDiscoveryDelegatingHandler : DelegatingHandler
    {
        private readonly ConsulClient _consulClient;
        private readonly ILogger<ConsulDiscoveryDelegatingHandler> _logger;
        public ConsulDiscoveryDelegatingHandler(ConsulClient consulClient,
              ILogger<ConsulDiscoveryDelegatingHandler> logger)
        {
            _consulClient = consulClient;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var current = request.RequestUri;
            try
            {
                //调用的服务地址里的域名(主机名)传入发现的服务名称即可
                request.RequestUri = new Uri($"{current.Scheme}://{LookupService(current.Host)}/{current.PathAndQuery}");
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger?.LogDebug(e, "Exception during SendAsync()");
                throw;
            }
            finally
            {
                request.RequestUri = current;
            }
        }

        private string LookupService(string serviceName)
        {
            var services = _consulClient.Catalog.Service(serviceName).Result.Response;
            if (services != null && services.Any())
            {
                //模拟负载均衡算法(随机获取一个地址)
                int index = new Random().Next(services.Count());
                var service = services.ElementAt(index);
                return $"{service.ServiceAddress}:{service.ServicePort}";
            }
            return null;
        }
    }
}
