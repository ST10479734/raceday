using Microsoft.AspNetCore.Mvc;
using raceday.database;
using raceday.models;
namespace raceday.Controllers
{// Start of namespace for controllers
    [Route("api/[controller]")]
    [ApiController]
    public class registrationController : ControllerBase
    {
        private readonly StoreUserInfo _storeUserInfo;
        // Constructor to initialize the database helper class
        public registrationController()
        {
            _storeUserInfo = new StoreUserInfo();
            
        }

        // HTTP POST endpoint for registering a new user via query parameters
        [HttpPost]
        public IActionResult Register([FromQuery] string fullname, [FromQuery] UserRole role, [FromQuery] string email, [FromQuery] string password)
        {
            // Validate incoming model state rules
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call database helper method to process user storage
            string result = _storeUserInfo.UserInfo(
                fullname,
                role,
                email,
                password
            );

            // Check if an error occurred during database operations
            if (result.StartsWith("Error"))
            {
                return StatusCode(500, new { Message = result });
            }

            // Return success response message
            return Ok(new { Message = result });
        }
    }
}