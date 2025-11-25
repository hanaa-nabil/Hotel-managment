using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.BLL.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otp);
    }
}
