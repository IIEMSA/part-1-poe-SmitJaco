using EventEase.Models;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Models
{
    public class EventEaseDBContext : DbContext
    {
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public EventEaseDBContext(DbContextOptions<EventEaseDBContext> options)
            : base(options)
        {
        }
    }
}