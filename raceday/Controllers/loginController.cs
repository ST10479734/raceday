using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;

namespace raceday.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class loginController : ControllerBase
    {
        private readonly StoreUserInfo _storeUserInfo;

        public loginController()
        {
            _storeUserInfo = new StoreUserInfo();
        }

        public loginController(StoreUserInfo storeUserInfo)
        {
            _storeUserInfo = storeUserInfo;
        }

        // HTTP POST endpoint for logging in using individual parameters
        [HttpPost]
        public IActionResult Login([FromQuery] string email, [FromQuery] string password)
        {
            // Validate that parameters are not empty
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { Message = "Email and password are required." });
            }

            // Call database helper method to validate credentials
            bool isValidUser = _storeUserInfo.ValidateUser(
                email,
                password,
                out string fullname,
                out string role
            );

            if (!isValidUser)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            // Return success response with user details
            return Ok(new
            {
                Message = "Login successful.",
                User = new
                {
                    Fullname = fullname,
                    Email = email,
                    Role = role
                }
            });
        }
    }
}
