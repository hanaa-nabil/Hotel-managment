using Hotel_Management.DAL.Entities;
using Microsoft.AspNetCore.Identity;
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
            builder.Entity<IdentityRole>().HasData(
       new IdentityRole
       {
           Id = "1",
           Name = "Admin",
           NormalizedName = "ADMIN",
           ConcurrencyStamp = Guid.NewGuid().ToString()
       },
       new IdentityRole
       {
           Id = "2",
           Name = "User",
           NormalizedName = "USER",
           ConcurrencyStamp = Guid.NewGuid().ToString()
       }
   );

            // Seed Admin User
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = "admin-user-id",
                UserName = "admin@hotel.com",
                NormalizedUserName = "ADMIN@HOTEL.COM",
                Email = "admin@hotel.com",
                NormalizedEmail = "ADMIN@HOTEL.COM",
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign Admin Role to Admin User
            builder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "1",
                    UserId = "admin-user-id"
                }
            );
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
