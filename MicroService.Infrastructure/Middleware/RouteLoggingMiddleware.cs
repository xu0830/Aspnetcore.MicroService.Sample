using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Middleware
{
    public class RouteLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RouteLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // 打印请求的路径
            Console.WriteLine($"Request Path: {context.Request.Path}");
            // 继续处理请求
            await _next(context);
        }
    }
}
