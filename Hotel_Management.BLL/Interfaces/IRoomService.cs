using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.BLL.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDTO>> GetAllRoomsAsync();
        Task<IEnumerable<RoomDTO>> GetRoomsByHotelIdAsync(int hotelId);
        Task<RoomDTO> GetRoomByIdAsync(int id);
        Task<RoomDTO> CreateRoomAsync(CreateRoomDTO model);
        Task<RoomDTO> UpdateRoomAsync(int id, CreateRoomDTO model);
        Task<bool> DeleteRoomAsync(int id);
        Task<IEnumerable<RoomDTO>> GetAvailableRoomsAsync(int hotelId, DateTime checkIn, DateTime checkOut);

    }
}
