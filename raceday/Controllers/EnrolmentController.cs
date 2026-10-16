using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;
using raceday.models;

namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrolmentController : ControllerBase
    {
        private readonly EnrolmentRepository _enrolmentRepository;

        public EnrolmentController()
        {
            _enrolmentRepository = new EnrolmentRepository();
        }

        // POST: api/enrolment/create
        [HttpPost("create")]
        public IActionResult CreateEnrolment(
            [FromForm] int userId,
            [FromForm] int categoryId,
            [FromForm] DateTime enrolmentDate,
            [FromForm] string raceNumber,
            [FromForm] PaymentStatus paymentStatus,
            [FromForm] string amount,
            [FromForm] DateTime paymentDate,
            [FromForm] PaymentMethod method)
        {
            // 1. Validate Form Inputs
            if (userId <= 0 || categoryId <= 0)
            {
                return BadRequest(new { message = "Valid User ID and Category ID are required." });
            }

            if (string.IsNullOrWhiteSpace(raceNumber))
            {
                return BadRequest(new { message = "Race number cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(amount))
            {
                return BadRequest(new { message = "Payment amount is required." });
            }

            // 2. Map form data to model
            EnrolmentModel model = new EnrolmentModel
            {
                UserId = userId,
                CategoryId = categoryId,
                EnrolmentDate = enrolmentDate,
                RaceNumber = raceNumber.Trim(),
                PaymentStatus = paymentStatus,
                Amount = amount.Trim(),
                PaymentDate = paymentDate,
                Method = method
            };

            // 3. Process database transaction
            string resultMessage = _enrolmentRepository.InsertEnrolment(model);

            // 4. Return HTTP Response
            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }
    }
}
