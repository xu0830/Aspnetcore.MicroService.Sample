using MicroService.Models;
using MicroService.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Repository
{
    public class AppDbContext : DbContext
    {
        private ILoggerFactory _LoggerFactory;
        private IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private IOptions<AppSetting> _appConfiguration;


        public AppDbContext(DbContextOptions<AppDbContext> options, ILoggerFactory loggerFactory,
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration,
            IOptions<AppSetting> appConfiguration)
            : base(options)
        {
            _LoggerFactory = loggerFactory;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _appConfiguration = appConfiguration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableSensitiveDataLogging(true); //允许打印参数
            optionsBuilder.UseLoggerFactory(_LoggerFactory);
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<SysUser> SysUsers { get; set; }

    }
}
