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
                options.SuppressModelStateInvalidFilter = true; // �����Զ�400��Ӧ
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


            app.UseMiddleware<GlobalExceptionMiddleware>();
            app.UseMiddleware<RouteLoggingMiddleware>();

            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    //context.Response.StatusCode = StatusCodes.Status400BadRequest; // ����״̬��Ϊ400 Bad Request
                    //context.Response.ContentType = "application/json"; // ������������Ϊ JSON
                    //var errors = context.Features.Get<IValidationProblemDetailsFeature>()?.Errors; // ��ȡ��֤������Ϣ
                    ////var result = new CustomValidationProblemDetails(errors); // �����Զ��巵�ؽ������
                    //await context.Response.WriteAsJsonAsync("error"); // �����д����Ӧ���в����� JSON ��ʽ�Ĵ�����Ϣ
                });
            });

            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    // ����������ȫ���쳣������
            //    app.UseExceptionHandler(errorApp =>
            //    {
            //        errorApp.Run(async context =>
            //        {
            //            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            //            context.Response.ContentType = "application/json";

            //            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerPathFeature>();
            //            var exception = exceptionHandlerFeature?.Error;

            //            // �Զ�����Ӧ��ʽ
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
