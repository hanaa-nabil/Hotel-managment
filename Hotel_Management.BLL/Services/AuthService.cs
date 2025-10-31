﻿using Hotel_Management.BLL.Interfaces;
using Hotel_Management.Common.Models.DTOs.AuthDTOS;
using Hotel_Management.DAL.Data;
using Hotel_Management.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace Hotel_Management.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO model)
        {
            if (model.Password != model.ConfirmPassword)
                return new AuthResponseDTO { Success = false, Message = "Passwords do not match" };

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            // Assign default "User" role
            await _userManager.AddToRoleAsync(user, "User");

            await GenerateAndSendOtpAsync(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Registration successful. OTP sent to email.",
                RequiresOtp = true,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new AuthResponseDTO { Success = false, Message = "Invalid credentials" };

            var result = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!result)
                return new AuthResponseDTO { Success = false, Message = "Invalid credentials" };

            await GenerateAndSendOtpAsync(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "OTP sent to email",
                RequiresOtp = true,
                Email = user.Email
            };
        }

        public async Task<AuthResponseDTO> VerifyOtpAsync(VerifyOtpDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new AuthResponseDTO { Success = false, Message = "User not found" };

            var otpCode = _context.OtpCodes
                .Where(o => o.UserId == user.Id && o.Code == model.Otp && !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (otpCode == null)
                return new AuthResponseDTO { Success = false, Message = "Invalid OTP" };

            if (otpCode.ExpiresAt < DateTime.UtcNow)
                return new AuthResponseDTO { Success = false, Message = "OTP expired" };

            otpCode.IsUsed = true;
            await _context.SaveChangesAsync();

            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Generate token with roles
            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "Login successful",
                Token = token,
                Email = user.Email,
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roles.ToList(),
                RequiresOtp = false
            };
        }

        public async Task<bool> ResendOtpAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return false;

            await GenerateAndSendOtpAsync(user);
            return true;
        }

        private async Task GenerateAndSendOtpAsync(ApplicationUser user)
        {
            var random = new Random();
            var otp = random.Next(1000, 9999).ToString();

            var otpCode = new OtpCode
            {
                UserId = user.Id,
                Code = otp,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            _context.OtpCodes.Add(otpCode);
            await _context.SaveChangesAsync();

            // Send OTP via email
            try
            {
                await _emailService.SendOtpEmailAsync(user.Email, otp);
                Console.WriteLine($"OTP sent successfully to {user.Email}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send OTP: {ex.Message}");
                // Still log to console for development
                Console.WriteLine($"OTP for {user.Email}: {otp}");
            }
        }

        private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            // Get user roles
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
            };

            // Add role claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
