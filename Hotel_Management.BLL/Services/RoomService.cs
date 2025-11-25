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
    public class RoomService : IRoomService
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly ApplicationDbContext _context;

        public RoomService(IRepository<Room> roomRepository, ApplicationDbContext context)
        {
            _roomRepository = roomRepository;
            _context = context;
        }

        public async Task<IEnumerable<RoomDTO>> GetAllRoomsAsync()
        {
            var rooms = await _context.Rooms.Include(r => r.Hotel).ToListAsync();
            return rooms.Select(r => MapToDTO(r));
        }

        public async Task<IEnumerable<RoomDTO>> GetRoomsByHotelIdAsync(int hotelId)
        {
            var rooms = await _context.Rooms.Include(r => r.Hotel)
                .ToListAsync();
            return rooms.Select(r => MapToDTO(r));
        }

        public async Task<RoomDTO> GetRoomByIdAsync(int id)
        {
            var room = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);
            return room != null ? MapToDTO(room) : null;
        }

        public async Task<RoomDTO> CreateRoomAsync(CreateRoomDTO model)
        {
            var room = new Room
            {
                HotelId = model.HotelId,
                RoomNumber = model.RoomNumber,
                Type = model.Type,
                PricePerNight = model.PricePerNight,
                IsAvailable = model.IsAvailable
            };

            var created = await _roomRepository.AddAsync(room);
            var roomWithHotel = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == created.Id);
            return MapToDTO(roomWithHotel);
        }

        public async Task<RoomDTO> UpdateRoomAsync(int id, CreateRoomDTO model)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return null;

            room.HotelId = model.HotelId;
            room.RoomNumber = model.RoomNumber;
            room.Type = model.Type;
            room.PricePerNight = model.PricePerNight;
            room.IsAvailable = model.IsAvailable;

            await _roomRepository.UpdateAsync(room);
            var roomWithHotel = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);
            return MapToDTO(roomWithHotel);
        }

        public async Task<bool> DeleteRoomAsync(int id)
        {
            return await _roomRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<RoomDTO>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut)
        {
            var bookedRoomIds = await _context.Bookings
                .Where(b => b.RoomId == hotelId &&
                           b.Status != "Cancelled" &&
                           ((b.CheckIn <= checkOut && b.CheckOut >= checkIn)))
                .Select(b => b.RoomId)
                .ToListAsync();

            var availableRooms = await _context.Rooms
                .Include(r => r.Hotel)
                .Where(r => r.HotelId == hotelId &&
                           r.IsAvailable &&
                           !bookedRoomIds.Contains(r.Id))
                .ToListAsync();

            return availableRooms.Select(r => MapToDTO(r));
        }

        private RoomDTO MapToDTO(Room room)
        {
            return new RoomDTO
            {
                Id = room.Id,
                HotelId = room.HotelId,
                RoomNumber = room.RoomNumber,
                Type = room.Type,
                PricePerNight = room.PricePerNight,
                IsAvailable = room.IsAvailable,
                HotelName = room.Hotel?.Name
            };
        }
    }
}
