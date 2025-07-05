using Consul;
using MicroService.Auth.Service;
using MicroService.Auth.Service.IService;
using MicroService.Infrastructure.HostService;
using MicroService.Models.Options;
using MicroService.Repository;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;

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

            //var connectionString = builder.Configuration.GetConnectionString("AppDbContext");
            //builder.Services.AddDbContext<AppDbContext>();
            builder.Services.AddScoped<ITokenService, TokenService>();
            //builder.Services.AddHostedService<ConsulRegistrationService>();

            //builder.Services.AddDiscoveryClient();

            //builder.Services.AddHttpClient("user", client =>
            //{
            //    client.BaseAddress = new Uri("http://user-service/");
            //});

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
