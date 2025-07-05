using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MicroService.Models.DTOS
{
    public record RegisterModel
    {
        [Required]
        public required string Account { get; set; }

        [Required]
        public required string Password { get; set; }

        [Required]
        public required string RepeatedPassword { get; set; }

        [Required]
        public required string Phone { get; set; }

        [Required]
        public required string RealName { get; set; }
    }
}
