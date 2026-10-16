using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;
using raceday.models;

namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class updateController : ControllerBase
    {
        private readonly StoreUserInfo _storeUserInfo;

        public updateController()
        {
            _storeUserInfo = new StoreUserInfo();
        }

        // HTTP PUT endpoint to edit user information via parameters
        [HttpPut("edit")]
        public IActionResult EditUser(
            [FromQuery] string email,
            [FromQuery] string fullname,
            [FromQuery] UserRole role,
            [FromQuery] string password)
        {
            // Validate incoming model state rules
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call database helper method to update user storage
            string result = _storeUserInfo.UpdateUser(
                email,
                fullname,
                role,
                password
            );

            // Check if an error occurred during database operations
            if (result.StartsWith("Error"))
            {
                if (result.Contains("not found"))
                {
                    return NotFound(new { Message = result });
                }
                return StatusCode(500, new { Message = result });
            }

            // Return success response message
            return Ok(new { Message = result });
        }
    }
}
