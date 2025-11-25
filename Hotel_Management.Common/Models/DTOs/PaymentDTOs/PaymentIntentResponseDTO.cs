using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class PaymentIntentResponseDTO
    {
        public string PaymentIntentId { get; set; }
        public string ClientSecret { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
    }
}
