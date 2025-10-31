using Hotel_Management.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_Management.DAL.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<OtpCode> OtpCodes { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Hotel> Hotels { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<OtpCode>()
                .HasIndex(o => new { o.UserId, o.Code })
                .IsUnique();

            SeedData(builder);
        }
        private static void SeedData(ModelBuilder builder)
        {
            // Seed ONE Hotel
            builder.Entity<Hotel>().HasData(
                new Hotel
                {
                    Id = 1,
                    Name = "COZY",
                    Address = "123 Main Street",
                    City = "Cairo",
                    Country = "Egypt",
                    Stars = 5
                }
            );

            builder.Entity<Room>().HasData(
                // Standard Rooms
                new Room { Id = 1, HotelId = 1, RoomNumber = "101", Type = "Standard", PricePerNight = 150.00m, IsAvailable = true },
                new Room { Id = 2, HotelId = 1, RoomNumber = "102", Type = "Standard", PricePerNight = 150.00m, IsAvailable = true },
                new Room { Id = 3, HotelId = 1, RoomNumber = "103", Type = "Standard", PricePerNight = 150.00m, IsAvailable = true },

                // Deluxe Rooms
                new Room { Id = 4, HotelId = 1, RoomNumber = "201", Type = "Deluxe", PricePerNight = 250.00m, IsAvailable = true },
                new Room { Id = 5, HotelId = 1, RoomNumber = "202", Type = "Deluxe", PricePerNight = 250.00m, IsAvailable = true },
                new Room { Id = 6, HotelId = 1, RoomNumber = "203", Type = "Deluxe", PricePerNight = 250.00m, IsAvailable = true },

                // Suite Rooms
                new Room { Id = 7, HotelId = 1, RoomNumber = "301", Type = "Suite", PricePerNight = 400.00m, IsAvailable = true },
                new Room { Id = 8, HotelId = 1, RoomNumber = "302", Type = "Suite", PricePerNight = 400.00m, IsAvailable = true },
                new Room { Id = 9, HotelId = 1, RoomNumber = "303", Type = "Suite", PricePerNight = 400.00m, IsAvailable = true },

                // Premium Suite
                new Room { Id = 10, HotelId = 1, RoomNumber = "401", Type = "Premium Suite", PricePerNight = 600.00m, IsAvailable = true }
            );
        }
    }
}
