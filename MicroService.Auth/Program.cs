using Consul;
using MicroService.Auth.Service;
using MicroService.Auth.Service.IService;
using MicroService.Infrastructure;
using MicroService.Infrastructure.Filter;
using MicroService.Infrastructure.HostService;
using MicroService.Infrastructure.Http;
using MicroService.Infrastructure.Middleware;
using MicroService.Models.Options;
using MicroService.Repository;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Steeltoe.Discovery.Client;
using Steeltoe.Discovery.Consul;
using Steeltoe.Extensions.Configuration;
using System.Text.Json;

namespace MicroService.Auth
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.Configure<ApiBehaviorOptions>(options => {
                options.SuppressModelStateInvalidFilter = true; // 禁用自动400响应
            });
            builder.Services.AddControllers(options => {
                options.Filters.Add(typeof(ValidateModelActionFilter));
            });

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


            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<RouteLoggingMiddleware>();

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    //context.Response.StatusCode = StatusCodes.Status400BadRequest; // 设置状态码为400 Bad Request
                    //context.Response.ContentType = "application/json"; // 设置内容类型为 JSON
                    //var errors = context.Features.Get<IValidationProblemDetailsFeature>()?.Errors; // 获取验证错误信息
                    ////var result = new CustomValidationProblemDetails(errors); // 创建自定义返回结果对象
                    //await context.Response.WriteAsJsonAsync("error"); // 将结果写入响应体中并返回 JSON 格式的错误信息
                });
            });

            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    // 生产环境：全局异常处理器
            //    app.UseExceptionHandler(errorApp =>
            //    {
            //        errorApp.Run(async context =>
            //        {
            //            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            //            context.Response.ContentType = "application/json";

            //            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            //            var exception = exceptionHandlerFeature?.Error;

            //            // 自定义响应格式
            //            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            //            {
            //                Code = context.Response.StatusCode,
            //                Message = "An unexpected error occurred",
            //                Path = exceptionHandlerFeature?.Path,
            //                Detail = app.Environment.IsDevelopment() ? exception?.ToString() : null
            //            }));
            //        });
            //    });
            //}


            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
