using MicroService.Models.DTOS;
using MicroService.User.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace MicroService.User.API.Controllers
{
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("loginGetUser")]
        public IActionResult LoginGetUser([FromBody] LoginModel model)
        {
            return Ok(_userService.LoginToGetUser(model));
        }
    }
}
