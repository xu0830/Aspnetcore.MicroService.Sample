using Consul;
using MicroService.Models.DTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Wrap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Http
{
    public class ResilienceHttpClient : IHttpClient
    {
        private readonly HttpClient _httpClient;

        //  根据 url origin 去创建 policy
        private readonly Func<string, IEnumerable<IAsyncPolicy>> _policyCreator;

        //  把 policy 打包成组合 policy wraper，进行本地缓存
        private readonly ConcurrentDictionary<string, AsyncPolicyWrap> _policyWrappers;

        private readonly ILogger<ResilienceHttpClient> _logger;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ResilienceHttpClient(Func<string, IEnumerable<IAsyncPolicy>> policyCreator,
            IHttpClientFactory clientFactory,
            ILogger<ResilienceHttpClient> logger)
        {
            _policyWrappers = new ConcurrentDictionary<string, AsyncPolicyWrap>();
            _policyCreator = policyCreator;

            //  DiscoveryRoundRobin、DiscoveryRandom
            _httpClient = clientFactory.CreateClient("DiscoveryRoundRobin");
            _logger = logger;
        }

        public Task<ResponseResult<R>> PostAsync<T, R>(string url, T item,
           string authorizationToken, string? requestId = null,
           string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync<R>(HttpMethod.Post, url, () => CreateHttpContent(item),
                authorizationToken, requestId, authorizationMethod);
        }
        public Task<ResponseResult<R>> PostAsync<R>(string url, Dictionary<string, string> form,
           string authorizationToken, string? requestId = null,
           string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync<R>(HttpMethod.Post, url, () => CreateHttpContent(form),
                authorizationToken, requestId, authorizationMethod);
        }
        private Task<ResponseResult<R>> DoPostPutAsync<R>(HttpMethod method, string url,
            Func<HttpContent> httpContent, string authorizationToken, string? requestId = null,
            string authorizationMethod = "Bearer")
        {
            if (method != HttpMethod.Post && method != HttpMethod.Put)
            {
                throw new ArgumentException("Value must be either post or put", nameof(method));
            }
            var origin = GetOrginFromUri(url);

            return HttpInvoker<R>(origin, async (c) => {
                var requestMessage = new HttpRequestMessage(method, url)
                {
                    Content = httpContent()
                };
                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                if (requestId != null)
                {
                    requestMessage.Headers.Add("x-requestid", requestId);
                }
                var response = await _httpClient.SendAsync(requestMessage);
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }
                return await response.Content.ReadFromJsonAsync<ResponseResult<R>>();
            });
        }
        private HttpContent CreateHttpContent<T>(T item)
        {
            return new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
        }
        private HttpContent CreateHttpContent(Dictionary<string, string> form)
        {
            return new FormUrlEncodedContent(form);
        }

        private static string NormalizeOrigin(string origin)
        {
            return origin?.Trim()?.ToLower();
        }
        private static string GetOrginFromUri(string uri)
        {
            var url = new Uri(uri);
            var origin = $"{url.Scheme}://{url.DnsSafeHost}:{url.Port}";
            return origin;
        }
        private void SetAuthorizationHeader(HttpRequestMessage requestMessage)
        {
            var authorizationHeader = _httpContextAccessor?.HttpContext?.Request?.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                requestMessage.Headers.Add("Authorization", new List<string> { authorizationHeader });
            }
        }
        public Task<ResponseResult<R>> GetAsync<R>(string url, string? authorizationToken = null, string authorizationMethod = "Bearer")
        {
            var origin = GetOrginFromUri(url);

            return HttpInvoker<R>(origin, async (c) => {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                SetAuthorizationHeader(requestMessage);

                if (authorizationToken != null)
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue(authorizationMethod, authorizationToken);
                }

                var response = await _httpClient.SendAsync(requestMessage);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new HttpRequestException();
                }
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException();
                }
                return await response?.Content?.ReadFromJsonAsync<ResponseResult<R>>();
            });
        }
        public Task<ResponseResult<R>> PutAsync<T, R>(string url, T item, string authorizationToken = null, string requestId = null, string authorizationMethod = "Bearer")
        {
            return DoPostPutAsync<R>(HttpMethod.Get, url, () => CreateHttpContent(item), authorizationToken, requestId, authorizationMethod);
        }
        private async Task<ResponseResult<R>> HttpInvoker<R>(string origin, Func<Context, Task<ResponseResult<R>>> func)
        {
            var normallizedOrigin = NormalizeOrigin(origin);
            if (!_policyWrappers.TryGetValue(normallizedOrigin, out AsyncPolicyWrap policyWrap))
            {
                policyWrap = Polly.Policy.WrapAsync(_policyCreator(normallizedOrigin).ToArray());
                _policyWrappers.TryAdd(normallizedOrigin, policyWrap);
            }
            return (await policyWrap.ExecuteAsync(func, new Context(normallizedOrigin)));
        }
    }
}
