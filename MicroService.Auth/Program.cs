using Consul;
using MicroService.Auth.Service;
using MicroService.Auth.Service.IService;
using MicroService.Infrastructure;
using MicroService.Infrastructure.Filter;
using MicroService.Infrastructure.HostService;
using MicroService.Infrastructure.Http;
using MicroService.Models.Options;
using MicroService.Repository;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;
using Steeltoe.Extensions.Configuration;

namespace MicroService.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            builder.Services.AddServiceDiscovery(o => o.UseConsul());

            //builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("ConsulConfig"));
            //builder.Services.AddSingleton<IConsulClient>(provider =>
            //    new ConsulClient(config => {
            //        config.Address = new Uri(provider.GetRequiredService<IOptions<ConsulConfig>>()?.Value?.ConsulUrl);
            //    })
            //);
            //builder.Services.AddHostedService<ConsulRegistrationService>();

            builder.Services.AddScoped<ITokenService, TokenService>();

            //builder.Services.AddHttpClient("user", client =>
            //{
            //    client.BaseAddress = new Uri("http://user-service/");
            //});

            builder.Services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var logger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var retryCount = 3;
                var exceptionCountAllowedBeforeBreaking = 3;
                return new ResilienceClientFactory(logger, httpClientFactory, retryCount, exceptionCountAllowedBeforeBreaking);
            });

            builder.Services.AddSingleton<IHttpClient>(sp =>
            {
                return sp.GetRequiredService<ResilienceClientFactory>().GetResilienceHttpClient();
            });

            builder.Services.AddMiniProfiler(options =>
            {
                // �趨���ʷ������URL��·�ɻ���ַ
                options.RouteBasePath = "/profiler";
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.PopupShowTimeWithChildren = true;
                options.PopupShowTrivial = true;
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                //  options.IgnoredPaths.Add("/swagger/");
            }).AddEntityFramework(); //��ʾSQL��估��ʱ

            var app = builder.Build();

            app.UseAuthorization();

            app.MapControllers();

            // �������˵�
            app.MapGet("/health", () => Results.Ok("Healthy"));

            app.Run();
        }
    }
}
