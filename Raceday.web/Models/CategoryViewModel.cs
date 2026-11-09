namespace Raceday.web.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
    }
}
