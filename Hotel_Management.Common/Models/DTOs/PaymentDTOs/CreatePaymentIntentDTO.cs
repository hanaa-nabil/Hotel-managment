using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class CreatePaymentIntentDTO
    {
        public int BookingId { get; set; }
        public decimal Amount { get; set; }
        public string? Currency { get; set; } = "usd";
        public string? Description { get; set; }
    }
}
