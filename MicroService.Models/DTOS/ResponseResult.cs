using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.DTOS
{
    public record ResponseResult<T>
    {
        public bool Status { get; set; }
        public string? Msg { get; set; }
        public T? Data { get; set; }
        public static ResponseResult<T> Result(bool _status, T? _data, string? _msg = null)
        {
            return new ResponseResult<T> { Status = _status, Data = _data, Msg = _msg };
        }
    }
}
