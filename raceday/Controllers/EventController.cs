using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;
using raceday.models;

namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        EventRepository _eventRepository;

        public EventController()
        {
            // Initialize the Data Handler (Repository) layer
            _eventRepository = new EventRepository();
        }

        // POST: api/event/create
        [HttpPost("create")]
        public IActionResult CreateEvent(
            [FromForm] int organizerId,
            [FromForm] string eventName,
            [FromForm] string location,
            [FromForm] DateTime eventDate)
        {
            // 1. Controller validation
            if (organizerId <= 0)
            {
                return BadRequest(new { message = "A valid Organizer ID is required." });
            }

            if (string.IsNullOrWhiteSpace(eventName))
            {
                return BadRequest(new { message = "Event name cannot be empty." });
            }

            if (string.IsNullOrWhiteSpace(location))
            {
                return BadRequest(new { message = "Event location cannot be empty." });
            }

            // 2. Populate the Model using the form parameters
            EventModel eventModel = new EventModel
            {
                OrganizerId = organizerId,
                EventName = eventName.Trim(),
                Location = location.Trim(),
                EventDate = eventDate
            };

            // 3. Delegate database insertion to the Data Handler
            string resultMessage = _eventRepository.InsertEvent(eventModel);

            // 4. Return appropriate HTTP response based on outcome
            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }

        [HttpPost("view-enrolments")]
        public IActionResult ViewOrganizerEnrolments([FromForm] int organizerId)
        {
            // 1. Validate Form Input
            if (organizerId <= 0)
            {
                return BadRequest(new { message = "A valid Organizer ID is required." });
            }

            // 2. Fetch enrolments for this organizer
            var enrolments = _eventRepository.GetEnrolmentsByOrganizerId(organizerId);

            // 3. Check if the list is empty or null
            if (enrolments == null || enrolments.Count == 0)
            {
                return Ok(new { message = "You have no registers or events." });
            }

            // 4. Return the list of enrolments if they exist
            return Ok(enrolments);
        }


        [HttpPut("update")]
        public IActionResult UpdateEvent(
                [FromForm] int eventId,
                [FromForm] int organizerId,
                [FromForm] string eventName,
                [FromForm] string location,
                [FromForm] DateTime eventDate)
        {
            if (eventId <= 0 || organizerId <= 0)
            {
                return BadRequest(new { message = "Valid Event ID and Organizer ID are required." });
            }

            if (string.IsNullOrWhiteSpace(eventName) || string.IsNullOrWhiteSpace(location))
            {
                return BadRequest(new { message = "Event name and location cannot be empty." });
            }

            EventModel eventModel = new EventModel
            {
                EventId = eventId,
                OrganizerId = organizerId,
                EventName = eventName.Trim(),
                Location = location.Trim(),
                EventDate = eventDate
            };

            string resultMessage = _eventRepository.UpdateEvent(eventModel);

            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }

        // DELETE: api/event/delete
        [HttpDelete("delete")]
        public IActionResult DeleteEvent(
            [FromForm] int eventId,
            [FromForm] int organizerId)
        {
            if (eventId <= 0 || organizerId <= 0)
            {
                return BadRequest(new { message = "Valid Event ID and Organizer ID are required." });
            }

            string resultMessage = _eventRepository.DeleteEvent(eventId, organizerId);

            if (resultMessage.Contains("successfully"))
            {
                return Ok(new { message = resultMessage });
            }

            return StatusCode(StatusCodes.Status500InternalServerError, new { message = resultMessage });
        }

       
    }
}
