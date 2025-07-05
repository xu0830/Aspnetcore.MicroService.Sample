using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.DTOS
{
    public class ApiResponse
    {
        public int? Code { get; }

        public object Data { get; }

        public ApiResponse(object data, int? _code)
        {
            Data = data;
            Code = _code.HasValue ? _code.Value : 200;
        }
    }
}
