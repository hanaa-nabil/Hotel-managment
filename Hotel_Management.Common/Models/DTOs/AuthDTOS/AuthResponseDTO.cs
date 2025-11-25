using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.AuthDTOS
{
    public class AuthResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public List<string> Roles { get; set; }
        public bool RequiresOtp { get; set; }
    }
}
