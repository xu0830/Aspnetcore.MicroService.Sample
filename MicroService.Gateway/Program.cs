using Consul;
using MicroService.Infrastructure.HostService;
using MicroService.Models.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;
using Ocelot.Provider.Polly;
using System.Text;

namespace MicroService.Gateway
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ���� ocelot.json ����
            builder.Configuration.AddJsonFile("ocelot.json");

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                // �Զ��� Token ��֤
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration?["Jwt:SecretKey"])),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // ��֤����ʱ��
                };

                // �Զ����¼�����
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"��֤ʧ��: {context.Exception}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.Configure<ConsulConfig>(builder.Configuration.GetSection("Consul"));
            builder.Services.AddSingleton<IConsulClient>(provider =>
                new ConsulClient(config =>
                {
                    var option = provider.GetRequiredService<IOptions<ConsulConfig>>()?.Value;
                    config.Address = new Uri($"{option?.Schema}://{option?.Host}:{option?.Port}");
                })
            );

            builder.Services.AddHostedService<ConsulRegistrationService>();

            // ��� Ocelot �� Consul ֧��
            builder.Services
                .AddOcelot()                
                .AddDelegatingHandler<OcelotExceptionHandler>()
                .AddConsul<CustomConsulServiceBuilder>()
                .AddPolly();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            var app = builder.Build();
            app.UseStaticFiles();
            
            app.UseAuthentication();
            if (app.Environment.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }


            //var configContent = File.ReadAllText("ocelot.json");

            //var consulClient = app?.Services?.GetService<IConsulClient>().KV;
            //await consulClient?.Put(new KVPair("ocelot/config")
            //{
            //    Value = Encoding.UTF8.GetBytes(configContent)
            //});

            await app.UseOcelot(config =>
            {
                config.PreErrorResponderMiddleware = async (ctx, next) =>
                {
                    //  �����˵���
                    if (ctx.Request.Path.Equals(new PathString("/health")))
                    {
                        await ctx.Response.WriteAsync("healthy");
                    }
                    else
                    {
                        await next.Invoke();
                    }
                };
                config.ResponderMiddleware = async (ctx, next) =>
                {
                    await next.Invoke();
                    var errors = ctx.Items.Errors();

                    if (errors?.Count > 0)
                    {
                        // �Զ��������Ӧ
                        await ctx.Response.WriteAsJsonAsync(new
                        {
                            Code = "ServiceUnavailable",
                            Errors = errors.Select(e => e.Message)
                        });
                    }
                };
            });

            app.Run();
        }
    }
}
