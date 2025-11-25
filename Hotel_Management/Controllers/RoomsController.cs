using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.HotelDTOS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rooms = await _roomService.GetAllRoomsAsync();
            return Ok(rooms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var room = await _roomService.GetRoomByIdAsync(id);
            if (room == null)
                return NotFound();

            return Ok(room);
        }


        [HttpGet("available/{hotelId}")]
        public async Task<IActionResult> GetAvailable(int hotelId, [FromQuery] DateTime checkIn, [FromQuery] DateTime checkOut)
        {
            var rooms = await _roomService.GetAvailableRoomsAsync(hotelId, checkIn, checkOut);
            return Ok(rooms);
        }

        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoomDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _roomService.CreateRoomAsync(model);
            return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateRoomDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var room = await _roomService.UpdateRoomAsync(id, model);
            if (room == null)
                return NotFound();

            return Ok(room);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
    }
}
