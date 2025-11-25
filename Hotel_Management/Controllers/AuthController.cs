using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.AuthDTOS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Hotel_Management.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyOtpAsync(model);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] string email)
        {
            var result = await _authService.ResendOtpAsync(email);

            if (!result)
                return BadRequest("Failed to resend OTP");

            return Ok("OTP sent successfully");
        }
    }
}
