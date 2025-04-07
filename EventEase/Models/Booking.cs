namespace EventEase.Models
{
    public class Booking
    {
        public int BookingId { get; set; } // Primary Key
        public int EventId { get; set; } // Foreign Key
        public int VenueId { get; set; } // Foreign Key
        public DateTime BookingDate { get; set; }
        public Event? Event { get; set; } // Navigation property 
        public Venue? Venue { get; set; } // Navigation property
    }
}
