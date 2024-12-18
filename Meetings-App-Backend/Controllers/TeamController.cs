using Meetings_App_Backend.Models;
using Meetings_App_Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Meetings_App_Backend.Controllers
{

    [Route("api")]
    [ApiController]
    public class TeamController:ControllerBase
    {
        private readonly IUserService _userService;

        public TeamController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/teams
        [Authorize]
        [HttpGet("teams")]
      

        // POST /api/teams
        [Authorize]
        [HttpPost("teams")]
        public async Task<IActionResult> CreateTeam([FromBody] AddTeamRequest model)
        {
            var team = await _userService.CreateTeamAsync(model);
            return Ok(team);
        }

        // PATCH /api/teams/{id}
        [Authorize]
        [HttpPatch("teams/{id}")]
        public async Task<IActionResult> UpdateTeam(int id, [FromQuery] string action, [FromQuery] string userId)
        {
            if (action == "add_member")
            {
                var updatedTeam = await _userService.AddMemberToTeamAsync(id, userId);
                return Ok(updatedTeam);
            }
            else if (action == "remove_member")
            {
                var updatedTeam = await _userService.RemoveMemberFromTeamAsync(id, userId);
                return Ok(updatedTeam);
            }
            else
            {
                return BadRequest("Invalid action.");
            }
        }
    }
}
