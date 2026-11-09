namespace Raceday.web.Models
{
    public class EventViewModel
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.Now.AddDays(7);
        public string? BannerImageUrl { get; set; }
        public IFormFile? BannerImageFile { get; set; }
        public int TotalEnrolments { get; set; }
        public List<CategoryViewModel> Categories { get; set; } = new();
    }
}
