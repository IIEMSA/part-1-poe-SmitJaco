using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Venue
    {
        public int VenueId { get; set; }
        public required string VenueName { get; set; } 
        public required string Location { get; set; }  
        public int Capacity { get; set; }
        // This stays â€” to store the URL of the image uploaded
        public string? ImageUrl { get; set; }

        // Add this new one â€” only for uploading from the Create/Edit form
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
        public List<Event>? Events { get; set; } 
    }
}