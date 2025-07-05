using Microsoft.AspNetCore.Diagnostics;

namespace MicroService.Gateway
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            await httpContext.Response.WriteAsync("发生未知错误");

            return await ValueTask.FromResult(false);
        }
    }
}
