using System.Net;
using System.Text.Json;

namespace MicroService.Gateway
{
    public class OcelotExceptionHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // 处理4xx/5xx状态码
                if (!response.IsSuccessStatusCode)
                {
                    return await TransformErrorResponse(response);
                }

                return response;
            }
            catch (HttpRequestException ex)
            {
                // 处理网络层异常
                return CreateErrorResponse(HttpStatusCode.BadGateway, "NETWORK_ERROR", ex.Message);
            }
            catch (TaskCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                // 处理超时
                return CreateErrorResponse(HttpStatusCode.GatewayTimeout, "TIMEOUT", "请求超时");
            }
        }

        private async Task<HttpResponseMessage> TransformErrorResponse(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            var errorResponse = new
            {
                Code = $"DOWNSTREAM_{response.StatusCode}",
                Message = "下游服务返回错误",
                OriginalStatus = (int)response.StatusCode,
                OriginalContent = content
            };

            return new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                Content = new StringContent(JsonSerializer.Serialize(errorResponse))
            };
        }

        private HttpResponseMessage CreateErrorResponse(
            HttpStatusCode statusCode,
            string errorCode,
            string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(JsonSerializer.Serialize(new
                {
                    Code = errorCode,
                    Message = message
                }))
            };
        }
    }
}
