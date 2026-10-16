using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;
using raceday.models;

namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResultController : ControllerBase
    {
        private readonly ResultRepository _resultRepository;

        public ResultController()
        {
            _resultRepository = new ResultRepository();
        }

        // POST: api/result/capture (For Organizers)
        [HttpPost("capture")]
        public IActionResult CaptureResult(
            [FromForm] int enrolmentId,
            [FromForm] TimeSpan finishTime,
            [FromForm] int overallPosition,
            [FromForm] int categoryPosition)
        {
            // 1. Validation
            if (enrolmentId <= 0)
            {
                return BadRequest(new { message = "A valid Enrolment ID is required." });
            }

            if (overallPosition <= 0 || categoryPosition <= 0)
            {
                return BadRequest(new { message = "Positions must be greater than zero." });
            }

            // 2. Map to Model
            ResultModel model = new ResultModel
            {
                EnrolmentId = enrolmentId,
                FinishTime = finishTime,
                OverallPosition = overallPosition,
                CategoryPosition = categoryPosition
            };

            // 3. Save to database
            string resultMessage = _resultRepository.InsertResult(model);

            // 4. Return response
            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }

        // POST: api/result/my-results (For Participants to view their own results)
        [HttpPost("my-results")]
        public IActionResult GetMyResults([FromForm] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest(new { message = "A valid User ID is required." });
            }

            var results = _resultRepository.GetResultsByUserId(userId);

            if (results == null || results.Count == 0)
            {
                return Ok(new { message = "You have no race results recorded." });
            }

            return Ok(results);
        }
    }
}