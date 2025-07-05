using MicroService.Models.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Http
{
    public interface IHttpClient
    {
        Task<ResponseResult<R>> PostAsync<T, R>(string url, T item, string? authorizationToken = null,
            string? requestId = null, string authorizationMethod = "Bearer");

        Task<ResponseResult<R>> PostAsync<R>(string url, Dictionary<string, string> form, string? authorizationToken = null,
           string? requestId = null, string authorizationMethod = "Bearer");

        Task<ResponseResult<R>> GetAsync<R>(string url, string? authorizationToken = null,
            string? authorizationMethod = "Bearer");

        Task<ResponseResult<R>> PutAsync<T, R>(string url, T item, string authorizationToken = null, string? requestId = null,
            string authorizationMethod = "Bearer");
    }
}
