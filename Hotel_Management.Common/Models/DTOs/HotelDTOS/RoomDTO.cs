using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.Common.Models.DTOs.HotelDTOS
{
    public class RoomDTO
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public string RoomNumber { get; set; }
        public string Type { get; set; }
        public decimal PricePerNight { get; set; }
        public bool IsAvailable { get; set; }
        public string HotelName { get; set; }
    }
}
