using MicroService.Models.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Infrastructure.Filter
{
    public class ValidateModelActionFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var errorResponse = ResponseResult<object>.Result(false, null, "参数错误");

                context.Result = new BadRequestObjectResult(errorResponse);
                return;
            }
            
            await next();
        }
    }
}
