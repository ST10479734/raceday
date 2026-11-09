using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;
using raceday.models;
//
namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentController : ControllerBase
    {
        private readonly EnrolmentRepositorys _enrolmentRepository;

        public EnrolmentController()
        {
            _enrolmentRepository = new EnrolmentRepositorys();
        }

        [HttpPost("view-enrolments")]
        public IActionResult ViewEnrolments([FromForm] int organizerId)
        {
            if (organizerId <= 0)
            {
                return BadRequest(new { message = "Invalid organizer ID." });
            }

            var enrolments = _enrolmentRepository.GetEnrolmentsByOrganizer(organizerId);

            if (enrolments == null || !enrolments.Any())
            {
                return Ok(new { message = "No enrolments found for your events." });
            }

            return Ok(enrolments);
        }

        // POST: api/enrolment/create
        [HttpPost("create")]
        public IActionResult CreateEnrolment(
    [FromForm] int userId,
    [FromForm] int categoryId,
    [FromForm] string amount)
        {
            // 1. Validate Form Inputs
            if (userId <= 0 || categoryId <= 0)
            {
                return BadRequest(new { message = "Valid User ID and Category ID are required." });
            }

            // 2. Auto-generate server-side tracking fields
            string generatedRaceNumber = "RN-" + new Random().Next(1000, 9999);

            // 3. Map form data and auto-assigned values to the model
            EnrolmentModel model = new EnrolmentModel
            {
                UserId = userId,
                CategoryId = categoryId,
                EnrolmentDate = DateTime.Now,
                RaceNumber = generatedRaceNumber,
                PaymentStatus = PaymentStatus.Paid, // Assuming PaymentStatus is your enum
                Amount = string.IsNullOrWhiteSpace(amount) ? "150.00" : amount.Trim(),
                PaymentDate = DateTime.Now,
                Method = PaymentMethod.Cash, // Default payment method
            };

            // 4. Process database transaction via repository
            string resultMessage = _enrolmentRepository.InsertEnrolment(model);

            // 5. Return HTTP Response
            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }
    }
}
