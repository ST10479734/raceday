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

        public registrationController()
        {
            _storeUserInfo = new StoreUserInfo();
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (result, userId) = _storeUserInfo.UserInfo(
                model.Fullname,
                model.Role,
                model.Email,
                model.Password
            );

            if (result.StartsWith("Error"))
            {
                return StatusCode(500, new { Message = result });
            }

            return Ok(new
            {
                Message = result,
                User = new
                {
                    UserId = userId,
                    Fullname = model.Fullname,
                    Email = model.Email,
                    Role = model.Role.ToString()
                }
            });
        }
    }
}