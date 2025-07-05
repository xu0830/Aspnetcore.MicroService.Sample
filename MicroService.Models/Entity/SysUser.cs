using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.Entity
{
    [Table("SysUser")]
    public record SysUser : BaseEntity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary>
        /// 用户登录帐号
        /// </summary>
        [Description("用户登录帐号")]
        public required string Account { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [Description("密码")]
        public required string Password { get; set; }

        /// <summary>
        /// 用户姓名
        /// </summary>
        [Description("真实姓名")]
        public required string RealName { get; set; }

        /// <summary>
        /// 手机号
        /// </summary>
        [Description("手机号")]
        public required string Phone { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        [Description("性别")]
        public int Sex { get; set; }

        /// <summary>
        /// 用户状态
        /// </summary>
        [Description("用户状态")]
        public int Status { get; set; }
        /// <summary>
        /// 业务对照码
        /// </summary>
        [Description("业务对照码")]
        public string? BizCode { get; set; }
        /// <summary>
        /// 经办时间
        /// </summary>
        [Description("创建时间")]
        public DateTime CreateTime { get; set; } = DateTime.Now; 

        /// <summary>
        /// 分类名称
        /// </summary>
        [Description("分类名称")]
        public string? TypeName { get; set; }

        /// <summary>
        /// 分类ID
        /// </summary>
        [Description("分类ID")]
        public string? TypeId { get; set; }

        /// <summary>
        /// 直接上级
        /// </summary>
        [Description("直接上级")]
        public string? ParentId { get; set; }

        public override void GenerateDefaultKeyVal()
        {
        }

        public override bool KeyIsNull()
        {
            return false;
        }
    }
}
