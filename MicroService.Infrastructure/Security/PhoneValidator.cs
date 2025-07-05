using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MicroService.Infrastructure.Security
{
    public static class PhoneValidator
    {
        public static bool IsValidPhoneNo(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }
            string pattern = @"^1[3-9]\d{9}$";
            return Regex.IsMatch(phone, pattern);
        }

    }
}
