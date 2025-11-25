using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class PaymentConfirmationDTO
    {
        public string PaymentIntentId { get; set; }
        public int BookingId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Status { get; set; }
        public string? ClientSecret { get; set; }
        public string? NextActionUrl { get; set; }
    }
}
