using MicroService.Models.DTOS;
using MicroService.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.User.Service.IService
{
    public interface IUserService
    {
        Task<SysUser> LoginToGetUserAsync(LoginModel loginModel);
        Task<bool> AddUser(RegisterModel user);
    }
}
