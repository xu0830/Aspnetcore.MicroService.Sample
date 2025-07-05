using MicroService.Infrastructure.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure
{
    public class ResilienceClientFactory
    {
        private ILogger<ResilienceHttpClient> _logger;
        private IHttpClientFactory _httpClientFactory;
        private int _retryCount;
        private int _exceptionCountAllowedBeforeBreaking;

        public ResilienceClientFactory(ILogger<ResilienceHttpClient> logger,
            IHttpClientFactory httpClientFactory,
            int retryCount,
            int exceptionCountAllowedBeforeBreaking)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _retryCount = retryCount;
            _exceptionCountAllowedBeforeBreaking = exceptionCountAllowedBeforeBreaking;
        }
        public ResilienceHttpClient GetResilienceHttpClient()
        {
            return new ResilienceHttpClient(origin => CreatePolicy(origin),
                _httpClientFactory, _logger);
        }

        private AsyncPolicy[] CreatePolicy(string origin)
        {
            return [
                Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(
                    _retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        var msg = $"第 {retryCount} 次重试 " +
                        $"of {context.PolicyKey} " +
                        $"at {context.PolicyKey}，" +
                        $"due to : {exception}. ";
                        _logger.LogWarning(msg);
                        _logger.LogDebug(msg);
                    }),
                Policy.Handle<HttpRequestException>()
                .CircuitBreakerAsync(
                    _exceptionCountAllowedBeforeBreaking,
                    TimeSpan.FromMinutes(1),
                    //  断路器打开时执行
                    (exception, duration) => {
                        _logger.LogWarning("断路器打开");
                    },
                    //断路器关闭
                    ()=>{
                        _logger.LogWarning("断路器关闭");
                    })
            ];
        }
    }
}
