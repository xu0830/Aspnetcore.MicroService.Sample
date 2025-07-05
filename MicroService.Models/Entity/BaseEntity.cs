using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.Entity
{
    public abstract class BaseEntity
    {
        /// <summary>
        /// 判断主键是否为空，常用做判定操作是【添加】还是【编辑】
        /// </summary>
        /// <returns></returns>
        public abstract bool KeyIsNull();

        /// <summary>
        /// 创建默认的主键值
        /// </summary>
        public abstract void GenerateDefaultKeyVal();

        public BaseEntity()
        {
            if (KeyIsNull())
            {
                GenerateDefaultKeyVal();
            }
        }
    }
}
