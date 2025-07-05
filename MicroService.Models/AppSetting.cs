using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models
{
    public class AppSetting
    {
        public AppSetting()
        {
            //SSOPassport = "http://localhost:52789";
            Version = "";
            UploadPath = "";
            IdentityServerUrl = "";
        }
        /// <summary>
        /// SSO地址
        /// </summary>
        //public string SSOPassport { get; set; }

        /// <summary>
        /// 版本信息
        /// 如果为demo,则屏蔽Post请求
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 数据库类型 SqlServer、MySql
        /// </summary>
        public Dictionary<string, string> DbTypes { get; set; } = new Dictionary<string, string>();

        /// <summary> 附件上传路径</summary>
        public string UploadPath { get; set; }

        //identity授权的地址
        public string IdentityServerUrl { get; set; }

        /// <summary>
        /// Redis服务器配置
        /// </summary>
        public string? RedisConf { get; set; }

        //是否是Identity授权方式
        public bool IsIdentityAuth => !string.IsNullOrEmpty(IdentityServerUrl);
    }
}
