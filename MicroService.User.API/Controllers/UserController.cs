using MicroService.Infrastructure.Extension;
using MicroService.Models.DTOS;
using MicroService.Models.Entity;
using MicroService.User.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.User.API.Controllers
{
    [Route("[controller]")]
    public class UserController : BaseController
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("loginGetUser")]
        public async Task<ResponseResult<SysUser>> LoginGetUser([FromBody] LoginModel model)
        {
            return ResultOk(await _userService.LoginToGetUserAsync(model));
        }

        [HttpPost("addUser")]
        public async Task<ResponseResult<object>> AddUser([FromBody] RegisterModel model)
        {
            var flag = await _userService.AddUser(model);
            return Result<object>(flag, null, flag ? "添加用户成功" : "添加用户失败");
        }
    }
}
