using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.DTOS
{
    public record LoginModel
    {
        [Required(ErrorMessage = "账号不能为空")]
        public required string Account { get; set; }

        [Required(ErrorMessage = "密码不能为空")]
        public required string Password { get; set; }
    }
}
