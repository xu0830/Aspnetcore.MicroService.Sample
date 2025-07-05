using Consul;
using MicroService.Infrastructure.HostService;
using MicroService.Models.Options;
using MicroService.Repository;
using MicroService.Repository.@interface;
using MicroService.User.Service.IService;
using MicroService.User.Service.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;
namespace MicroService.User.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();

            //builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("ConsulConfig"));
            //builder.Services.AddSingleton<IConsulClient>(provider =>
            //    new ConsulClient(config => {
            //        config.Address = new Uri(provider.GetRequiredService<IOptions<ConsulConfig>>()?.Value?.ConsulUrl);
            //    })
            //);
            //builder.Services.AddHostedService<ConsulRegistrationService>();

            builder.Services.AddServiceDiscovery(o => o.UseConsul());

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDbContext<AppDbContext>(options => 
                options.UseMySql(builder.Configuration.GetConnectionString("AppDbContext"), new MySqlServerVersion(new Version(8, 0, 11))));
            
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped(typeof(IUnitWork<>), typeof(UnitWork<>));
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
