using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.HotelDTOS
{
    public class BookingWithPaymentDTO
    {
        public int BookingId { get; set; }
        public decimal TotalAmount { get; set; }
        public string ClientSecret { get; set; }
        public string PaymentIntentId { get; set; }
    }
}
