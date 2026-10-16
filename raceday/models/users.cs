using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace raceday.models
{// Start of namespace for models and data transfer objects

    // Converter attribute to ensure Swagger displays enums as text strings instead of numbers
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum UserRole
    {
        Organiser,
        Participant
    }

    // Data Transfer Object representing user registration inputs with validation constraints
    public class UserRegisterDto
    {
        [Required]
        public string Fullname { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
    // Data Transfer Object representing user login inputs
    public class UserLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class EventModel
    {
        public int EventId { get; set; }
        public int OrganizerId { get; set; }
        public string EventName { get; set; }
        public string Location { get; set; }
        public DateTime EventDate { get; set; }
        
    }

    public class CategoryModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int EventId { get; set; }
        public string Distance { get; set; }
        public string EventType { get; set; }
    }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Refunded
    }

    // Define the Payment Method enum
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PaymentMethod
    {
        CreditCard,
        DebitCard,
        PayPal,
        Cash,
        BankTransfer
    }
    public class EnrolmentModel
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string RaceNumber { get; set; }

        // Use the enums here
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod Method { get; set; }

        // Payment fields
        public string Amount { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public class EnrolmentDetailModel
    {
        public int EnrolmentId { get; set; }
        public int UserId { get; set; }
        public string ParticipantName { get; set; }
        public string ParticipantEmail { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Distance { get; set; }
        public string EventType { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; }
        public DateTime EnrolmentDate { get; set; }
        public string RaceNumber { get; set; }
        public string PaymentStatus { get; set; }
        public string Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Method { get; set; }
    }

    public class ResultModel
    {
        public int ResultsId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }

        // Additional fields for viewing results conveniently
        public string EventName { get; set; }
        public string CategoryName { get; set; }
        public string RaceNumber { get; set; }
        public string ParticipantName { get; set; }
    }

}// End of models namespace
