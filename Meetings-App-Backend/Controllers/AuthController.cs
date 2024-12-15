using Meetings_App_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Meetings_App_Backend.Models;

namespace Meetings_App_Backend.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (await _userService.UserExistsAsync(model.Email))
                return BadRequest("User already exists.");

            var user = await _userService.RegisterAsync(model);
            return Ok(new { message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            var user = await _userService.AuthenticateAsync(model);
            if (user == null) return Unauthorized("Invalid credentials.");

            var token = _userService.GenerateJwtToken(user);
            return Ok(new { message = "Signed in successfully", token });
        }
    }
}
