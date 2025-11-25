using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.HotelDTOS
{
    public class BookingResponseDTO
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int NumberOfNights { get; set; }
        public decimal PricePerNight { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public bool IsPaid { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public string Hotelname { get; set; } = "Cozy Hotel";
    }

}
