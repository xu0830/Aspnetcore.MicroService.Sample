using MicroService.Infrastructure.Security;
using MicroService.Models.DTOS;
using MicroService.Models.Entity;
using MicroService.Models.Options;
using MicroService.Repository;
using MicroService.Repository.@interface;
using MicroService.User.Service.IService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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

        public async Task<bool> AddUser(RegisterModel user)
        {
            var _user = await _unitWork.AddAsync(new SysUser
            {
                Account = user.Account,
                RealName = user.RealName,
                Password = Pbkdf2Hasher.HashPassword(user.Password),
                Phone = user.Phone
            });

            return await _unitWork.SaveAsync() > 0;
        }

        public async Task<SysUser> LoginToGetUserAsync(LoginModel model)
        {
            var user = await _unitWork.FirstOrDefaultAsync<SysUser>(c => c.Account == model.Account);
            if (user == null || !Pbkdf2Hasher.VerifyPassword(model.Password, user.Password))
            {
                return null;
            }
            return user;
        }
    }
}
