using AutoMapper;
using Meetings_App_Backend.Data;
using Meetings_App_Backend.Models;
using Meetings_App_Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Meetings_App_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;


        public CalendarController(ApplicationDbContext context, IUserService userService, IMapper mapper)
        {
            _context = context;
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetMeetingsForDay([FromQuery] DateTime date)
        {
            // Get the current user's ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated" });
            }

            // Filter the meetings for the given date where the logged-in user is an attendee
            var meetings = await _context.Meetings
                .Where(m => m.MeetingAttendees.Any(ma => ma.UserId.ToString() == currentUserId) &&
                            m.Date == date.Date)  // Matching the date
                .Include(m => m.MeetingAttendees)
                .ThenInclude(ma => ma.User)
                .ToListAsync();

            if (meetings == null || !meetings.Any())
            {
                return NotFound(new { Message = "No meetings found for the given date." });
            }

            // Map the meetings to the response model (assuming you have a MeetingResponse mapping)
            var response = _mapper.Map<List<MeetingResponse>>(meetings);

            return Ok(response);
        }
    }
}
