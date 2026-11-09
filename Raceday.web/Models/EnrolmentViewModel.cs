namespace Raceday.web.Models
{
    public class EnrolmentViewModel
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public string ParticipantName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime EnrolmentDate { get; set; } = DateTime.Now;
        public string RaceNumber { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Confirmed";
        public string Amount { get; set; } = "0.00";
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string Method { get; set; } = "CreditCard";
    }
}
