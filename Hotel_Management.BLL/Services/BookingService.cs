using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using Hotel_Management.DAL.Data;
using Hotel_Management.DAL.Entities;
using Hotel_Management.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.BLL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly ApplicationDbContext _context;

        public BookingService(IRepository<Booking> bookingRepository, ApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<IEnumerable<BookingDTO>> GetAllBookingsAsync()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .ToListAsync();
            return bookings.Select(b => MapToDTO(b));
        }

        public async Task<IEnumerable<BookingDTO>> GetUserBookingsAsync(string userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .Where(b => b.UserId == userId)
                .ToListAsync();
            return bookings.Select(b => MapToDTO(b));
        }

        public async Task<BookingDTO> GetBookingByIdAsync(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(b => b.Id == id);
            return booking != null ? MapToDTO(booking) : null;
        }

        public async Task<BookingDTO> CreateBookingAsync(string userId, CreateBookingDTO model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null || !room.IsAvailable)
                throw new Exception("Room is not available");

            // Check if room is already booked for these dates
            var isBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == model.RoomId &&
                b.Status != "Cancelled" &&
                ((b.CheckIn <= model.CheckOut && b.CheckOut >= model.CheckIn)));

            if (isBooked)
                throw new Exception("Room is already booked for these dates");

            var nights = (model.CheckOut - model.CheckIn).Days;
            var totalPrice = nights * room.PricePerNight;

            var booking = new Booking
            {
                UserId = userId,
                RoomId = model.RoomId,
                CheckIn = model.CheckIn,
                CheckOut = model.CheckOut,
                TotalPrice = totalPrice,
                Status = "Confirmed"
            };

            var created = await _bookingRepository.AddAsync(booking);
            var bookingWithDetails = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(b => b.Id == created.Id);

            return MapToDTO(bookingWithDetails);
        }

        public async Task<bool> CancelBookingAsync(int id, string userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null || booking.UserId != userId)
                return false;

            booking.Status = "Cancelled";
            await _bookingRepository.UpdateAsync(booking);
            return true;
        }

        public async Task<BookingDTO> UpdateBookingStatusAsync(int id, string status)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return null;

            booking.Status = status;
            await _bookingRepository.UpdateAsync(booking);

            var bookingWithDetails = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(b => b.Id == id);

            return MapToDTO(bookingWithDetails);
        }

        private BookingDTO MapToDTO(Booking booking)
        {
            return new BookingDTO
            {
                Id = booking.Id,
                UserId = booking.UserId,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber,
                HotelName = booking.Room?.Hotel?.Name,
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status
            };
        }
    }
}
