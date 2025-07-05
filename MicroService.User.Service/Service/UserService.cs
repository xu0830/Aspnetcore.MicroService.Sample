using MicroService.Models.DTOS;
using MicroService.Models.Entity;
using MicroService.Repository;
using MicroService.Repository.@interface;
using MicroService.User.Service.IService;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.User.Service.Service
{
    public class UserService : IUserService
    {
        private readonly IUnitWork<AppDbContext> _unitWork;

        public UserService(IUnitWork<AppDbContext> unitWork) 
        {
            _unitWork = unitWork;
        }

        public SysUser LoginToGetUser(LoginModel model)
        {
            var user = _unitWork.FirstOrDefault<SysUser>(c => c.Account == model.Account && c.Password == model.Password);
            return user;
        }
    }
}
