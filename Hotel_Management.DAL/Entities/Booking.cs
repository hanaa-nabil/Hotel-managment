using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hotel_Management.DAL.Entities
{
    public class Booking
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual Room Room { get; set; }
        public string? StripePaymentIntentId { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaymentDate { get; set; }
        public decimal? RefundAmount { get; set; }

    }
}
