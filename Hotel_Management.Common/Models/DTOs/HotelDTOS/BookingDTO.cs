using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.HotelDTOS
{
    public class BookingDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string HotelName { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public int NumberOfNights { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsPaid { get; set; }
        public string? StripePaymentIntentId { get; set; }

    }
}
