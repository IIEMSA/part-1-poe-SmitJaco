namespace EventEase.Models
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string VenueName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }
}