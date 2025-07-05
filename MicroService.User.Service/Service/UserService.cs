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

        private readonly AESEncryptOption _encryptOption;

        public UserService(IUnitWork<AppDbContext> unitWork, IOptions<AESEncryptOption> encryptOption) 
        {
            _unitWork = unitWork;
            _encryptOption = encryptOption.Value;
        }

        public async Task<bool> AddUser(RegisterModel user)
        {
            var _user = await _unitWork.AddAsync(new SysUser
            {
                Account = user.Account,
                RealName = user.RealName,
                Password = EncryUtils.AESEncrypt(user.Password, _encryptOption.Secret),
                Phone = user.Phone
            });

            return await _unitWork.SaveAsync() > 0;
        }

        public async Task<SysUser> LoginToGetUserAsync(LoginModel model)
        {
            model.Password = EncryUtils.AESEncrypt(model.Password, _encryptOption.Secret);
            return await _unitWork.FirstOrDefaultAsync<SysUser>(c => c.Account == model.Account && c.Password == model.Password);
        }
    }
}
