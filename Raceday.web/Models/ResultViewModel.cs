using System.ComponentModel.DataAnnotations;

namespace Raceday.web.Models
{
    public class ResultViewModel
    {
        public int ResultId { get; set; }

        [Required(ErrorMessage = "Enrolment ID is required.")]
        public int EnrolmentId { get; set; }

        [Display(Name = "Event Name")]
        public string EventName { get; set; } = string.Empty;

        [Display(Name = "Event Date")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Race Number")]
        public string RaceNumber { get; set; } = string.Empty;

        [Display(Name = "Category")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Finish time is required.")]
        [Display(Name = "Finish Time")]
        public TimeSpan FinishTime { get; set; }

        [Required(ErrorMessage = "Overall position is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
        [Display(Name = "Overall Position")]
        public int OverallPosition { get; set; }

        [Required(ErrorMessage = "Category position is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Position must be greater than zero.")]
        [Display(Name = "Category Position")]
        public int CategoryPosition { get; set; }

        [Display(Name = "Total Finishers")]
        public int TotalFinishers { get; set; }
    }
}