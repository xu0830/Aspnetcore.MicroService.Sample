using Consul;
using MicroService.Infrastructure.Filter;
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

            #region consul 依赖注入、服务注册
            //  方式1、依赖注入、Hostservice 服务注册、健康检查 自动重连
            //  服务间调用通过DelegatingHandler将请求地址转发到实际的服务地址端口
            //builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("ConsulConfig"));
            //builder.Services.AddSingleton<IConsulClient>(provider =>
            //    new ConsulClient(config => {
            //        config.Address = new Uri(provider.GetRequiredService<IOptions<ConsulConfig>>()?.Value?.ConsulUrl);
            //    })
            //);
            //builder.Services.AddHostedService<ConsulRegistrationService>();

            //  方式2、基于 Steeltoe.Discovery.Consul 实现的 服务注册（自动重连）、发现
            builder.Services.AddServiceDiscovery(o => o.UseConsul());
            #endregion

            builder.Services.AddHttpContextAccessor();

            #region 数据库服务
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(builder.Configuration.GetConnectionString("AppDbContext"), new MySqlServerVersion(new Version(8, 0, 11))));
            builder.Services.AddScoped(typeof(IUnitWork<>), typeof(UnitWork<>));
            #endregion

            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddMiniProfiler(options =>
            {
                // 设定访问分析结果URL的路由基地址
                options.RouteBasePath = "/profiler";
                options.ColorScheme = StackExchange.Profiling.ColorScheme.Auto;
                options.PopupRenderPosition = StackExchange.Profiling.RenderPosition.BottomLeft;
                options.PopupShowTimeWithChildren = true;
                options.PopupShowTrivial = true;
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                //  options.IgnoredPaths.Add("/swagger/");
            }).AddEntityFramework(); //显示SQL语句及耗时

            var app = builder.Build();

            app.UseAuthorization();

            app.MapControllers();

            // 健康检查端点
            app.MapGet("/health", () => Results.Ok("Healthy"));

            app.Run();
        }
    }
}
