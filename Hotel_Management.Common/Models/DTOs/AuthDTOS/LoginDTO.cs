using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.AuthDTOS
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
