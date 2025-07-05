using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.Options
{
    public class AESEncryptOption
    {
        public required string Secret { get; set; }
    }
}
