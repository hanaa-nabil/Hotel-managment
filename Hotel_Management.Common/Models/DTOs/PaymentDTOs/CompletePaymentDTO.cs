using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.PaymentDTOs
{
    public class CompletePaymentDTO
    {
        public string PaymentIntentId { get; set; }
        public int BookingId { get; set; }
    }
}
