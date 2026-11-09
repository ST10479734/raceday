using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using raceday.database;
//
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

        [HttpPost]
        public IActionResult Login([FromQuery] string email, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return BadRequest(new { Message = "Email and password are required." });
            }

            bool isValidUser = _storeUserInfo.ValidateUser(
                email,
                password,
                out int userId,
                out string fullname,
                out string role
            );

            if (!isValidUser)
            {
                return Unauthorized(new { Message = "Invalid email or password." });
            }

            return Ok(new
            {
                Message = "Login successful.",
                User = new
                {
                    UserId = userId,
                    Fullname = fullname,
                    Email = email,
                    Role = role
                }
            });
        }
    }
}