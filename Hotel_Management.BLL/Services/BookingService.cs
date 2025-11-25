using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using Hotel_Management.Common.Models.DTOs.PaymentDTOs;
using Hotel_Management.DAL.Data;
using Hotel_Management.DAL.Entities;
using Hotel_Management.DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Hotel_Management.BLL.Services
{
    public class BookingService : IBookingService
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Room> _roomRepository;

        public BookingService(
            IRepository<Booking> bookingRepository,
            ApplicationDbContext context,
            IRepository<Room> roomRepository)
        {
            _bookingRepository = bookingRepository;
            _context = context;
            _roomRepository = roomRepository;
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

        // DEPRECATED: Old method for backward compatibility
        // Use CreateBookingWithPaymentAsync instead
        public async Task<BookingDTO> CreateBookingAsync(string userId, CreateBookingDTO model)
        {
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null || !room.IsAvailable)
                throw new Exception("Room is not available");

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
                Status = "Pending", // Changed from "Confirmed" - should be pending until payment
                IsPaid = false
            };

            var created = await _bookingRepository.AddAsync(booking);
            var bookingWithDetails = await _context.Bookings
                .Include(b => b.Room)
                .ThenInclude(r => r.Hotel)
                .FirstOrDefaultAsync(b => b.Id == created.Id);

            return MapToDTO(bookingWithDetails);
        }

        public async Task<bool> CancelBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking == null)
                return false;

            booking.Status = "Cancelled";
            await _bookingRepository.UpdateAsync(booking);

            return true;
        }

        public async Task<string> GetPaymentIntentIdAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            return booking?.StripePaymentIntentId;
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

        // REMOVED: Duplicate CreateBookingAsync that returns BookingResponseDTO

        // PRIMARY METHOD: Use this for new bookings with payment
        public async Task<BookingWithPaymentDTO> CreateBookingWithPaymentAsync(CreateBookingDTO dto, string userId)
        {
            var room = await _context.Rooms.FindAsync(dto.RoomId);
            if (room == null)
                throw new Exception("Room not found");

            if (!room.IsAvailable)
                throw new Exception("Room is not available");

            var isBooked = await _context.Bookings.AnyAsync(b =>
                b.RoomId == dto.RoomId &&
                b.Status != "Cancelled" &&
                ((b.CheckIn <= dto.CheckOut && b.CheckOut >= dto.CheckIn)));

            if (isBooked)
                throw new Exception("Room is already booked for these dates");

            var totalAmount = await CalculateTotalAmountAsync(dto.RoomId, dto.CheckIn, dto.CheckOut);

            var booking = new Booking
            {
                UserId = userId,
                RoomId = dto.RoomId,
                CheckIn = dto.CheckIn,
                CheckOut = dto.CheckOut,
                TotalPrice = totalAmount,
                Status = "Pending",
                IsPaid = false
            };

            var created = await _bookingRepository.AddAsync(booking);

            return new BookingWithPaymentDTO
            {
                BookingId = created.Id,
                TotalAmount = totalAmount,
                ClientSecret = null,
                PaymentIntentId = null
            };
        }

        public async Task<decimal> CalculateTotalAmountAsync(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
                throw new Exception("Room not found");

            var numberOfNights = (checkOut - checkIn).Days;
            if (numberOfNights <= 0)
                throw new Exception("Check-out date must be after check-in date");

            return room.PricePerNight * numberOfNights;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int bookingId, string paymentIntentId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return false;
            }
            booking.StripePaymentIntentId = paymentIntentId;
            booking.IsPaid = true;
            booking.Status = "Confirmed";

            await _bookingRepository.UpdateAsync(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        private BookingDTO MapToDTO(Booking booking)
        {
            var numberOfNights = (booking.CheckOut - booking.CheckIn).Days;

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
                Status = booking.Status,
                IsPaid = booking.IsPaid,
                StripePaymentIntentId = booking.StripePaymentIntentId,
                NumberOfNights = numberOfNights,
                PricePerNight = booking.Room?.PricePerNight ?? 0
            };
        }

        private BookingResponseDTO MapToBookingResponseDTO(Booking booking)
        {
            var numberOfNights = (booking.CheckOut - booking.CheckIn).Days;

            return new BookingResponseDTO
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomNumber = booking.Room?.RoomNumber,
                RoomType = booking.Room?.Type,
                Hotelname = booking.Room?.Hotel?.Name,
                CheckIn = booking.CheckIn,
                CheckOut = booking.CheckOut,
                NumberOfNights = numberOfNights,
                PricePerNight = booking.Room?.PricePerNight ?? 0,
                TotalAmount = booking.TotalPrice,
                Status = booking.Status,
                IsPaid = booking.IsPaid,
                StripePaymentIntentId = booking.StripePaymentIntentId
            };
        }
    }
}