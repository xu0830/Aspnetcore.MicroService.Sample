using Consul;
using MicroService.Models.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;

namespace MicroService.Infrastructure.HostService
{
    /// <summary>
    /// consul服务注册，健康检查自动重连
    /// </summary>
    public class ConsulRegistrationService : IHostedService
    {
        private readonly IConsulClient _consulClient;
        private readonly ConsulConfig? _config;
        private readonly ILogger<ConsulRegistrationService> _logger;
        private readonly AgentServiceRegistration? _registration;
        private Timer? _healthTimer;
        private bool _isRegistered = false;

        public ConsulRegistrationService(
            IConsulClient consulClient,
            IOptions<ConsulConfig> config,
            ILogger<ConsulRegistrationService> logger)
        {
            _consulClient = consulClient;
            _config = config.Value;
            _logger = logger;

            _registration = new AgentServiceRegistration
            {
                ID = _config?.Discovery?.InstanceId,
                Name = _config?.Discovery?.ServiceName,
                Address = _config?.Discovery?.Hostname,
                Port = _config.Discovery.Port,
                Tags = _config.Discovery?.Tags,
                Check = new AgentServiceCheck
                {
                    HTTP = $"{_config?.Discovery?.Schema}://{_config?.Discovery?.Hostname}:{_config.Discovery.Port}{_config.Discovery.HealthCheckUrl}",
                    Interval = TimeSpan.FromSeconds(10),
                    Timeout = TimeSpan.FromSeconds(5),
                    DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(30)
                }
            };
        }


        private async Task RegisterServiceAsync()
        {
            try
            {
                var result = await _consulClient.Agent.ServiceRegister(_registration);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    _isRegistered = true;
                    _logger.LogInformation($"【consul】服务注册成功: {_config?.Discovery?.ServiceName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "【consul】服务注册失败");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _healthTimer?.Dispose();

            if (_isRegistered && _registration != null)
            {
                await _consulClient.Agent.ServiceDeregister(_registration.ID);
                _logger.LogInformation("【consul】服务已注销");
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            // 初始注册
            await RegisterServiceAsync();

            // 定时检查注册状态
            _healthTimer = new Timer(async _ =>
            {
                try
                {
                    var response = await _consulClient.Health.Service(
                        _registration?.Name,
                        _registration?.Tags?[0],
                        passingOnly: true,
                        ct: cancellationToken);

                    if (response.Response == null || !response.Response.Any())
                    {
                        _isRegistered = false;
                        _logger.LogWarning("【consul】服务未注册，尝试重新注册...");
                        await RegisterServiceAsync();
                    }
                    else
                    {
                        _logger.LogDebug($"【consul】服务健康检查：正常");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "【consul】自动重新注册异常");
                }
            }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)); // 每分钟检查一次
        }
    }
}
