using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.BLL.Interfaces
{
    public interface IBookingService
    {
        Task<IEnumerable<BookingDTO>> GetAllBookingsAsync();
        Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(string userId);
        Task<BookingDTO> GetBookingByIdAsync(int id);
        Task<BookingDTO> CreateBookingAsync(string userId, CreateBookingDTO model);
        Task<bool> CancelBookingAsync(int id, string userId);
        Task<BookingDTO> UpdateBookingStatusAsync(int id, string status);
    }
}
