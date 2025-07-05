using MicroService.Models.DTOS;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Extension
{
    public class BaseController : ControllerBase
    {
        protected ResponseResult<T> Result<T>(bool status, T? data, string? msg = null)
        {
            return ResponseResult<T>.Result(status, data, msg);
        }
        protected ResponseResult<T> ResultOk<T>(T? data, string? msg = null)
        {
            return ResponseResult<T>.Result(true, data, msg);
        }

        protected ResponseResult<T> ResultFail<T>(string msg)
        {
            return ResponseResult<T>.Result(false, default, msg);
        }
    }
}
