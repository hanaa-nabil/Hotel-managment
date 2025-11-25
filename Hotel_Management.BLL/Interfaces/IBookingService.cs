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
        Task<BookingDTO> UpdateBookingStatusAsync(int id, string status);

        //Task<BookingResponseDTO> CreateBookingAsync(CreateBookingDTO dto, string userId);
        Task<BookingWithPaymentDTO> CreateBookingWithPaymentAsync(CreateBookingDTO dto, string userId);
        Task<bool> UpdatePaymentStatusAsync(int bookingId, string paymentIntentId);//, bool isPaid);
        Task<bool> CancelBookingAsync(int bookingId);
        Task<decimal> CalculateTotalAmountAsync(int roomId, DateTime checkIn, DateTime checkOut);

        // Add to IBookingService interface
        Task<string> GetPaymentIntentIdAsync(int bookingId);
    }
}
