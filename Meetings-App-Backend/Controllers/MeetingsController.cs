using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meetings_App_Backend.Data;
using Meetings_App_Backend.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
namespace Meetings_App_Backend.Controllers
{
    [Route("api/meetings")]
    [ApiController]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public MeetingsController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> CreateMeetings([FromBody] Meetings meeting)
        {
            var userId = int.Parse(User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Add the meeting to the database
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Create a new MeetingAttendee to add the logged-in user to the meeting
            var meetingAttendee = new MeetingAttendee
            {
                MeetingId = meeting.Id,
                UserId = user.Id
            };

            _context.MeetingAttendees.Add(meetingAttendee);
            await _context.SaveChangesAsync();

            return Ok(meeting);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetMeetings()
        {
            var meetings = await _context.Meetings
                .Include(m => m.Attendees)
                .ThenInclude(ma => ma.User)
                .ToListAsync();

            return Ok(meetings);
        }
        [HttpPatch("{meetingId}/add-attendee")]
        public async Task<IActionResult> AddAttendee(int meetingId, [FromBody] AddAttendeeDto attendeeDto)
        {
            // Check if the user exists in the database
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == attendeeDto.Email);
            if (user == null)
            {
                return BadRequest("User not registered.");
            }

            // Check if the meeting exists
            var meeting = await _context.Meetings.FindAsync(meetingId);
            if (meeting == null)
            {
                return NotFound("Meeting not found.");
            }

            // Check if the user is already an attendee
            var existingAttendee = await _context.MeetingAttendees
                .AnyAsync(ma => ma.MeetingId == meetingId && ma.UserId == user.Id);

            if (existingAttendee)
            {
                return BadRequest("User is already an attendee.");
            }

            // Add the user as an attendee
            var meetingAttendee = new MeetingAttendee
            {
                MeetingId = meetingId,
                UserId = user.Id
            };

            _context.MeetingAttendees.Add(meetingAttendee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User added as attendee." });
        }
    }
    public class AddAttendeeDto
    {
        public string Email { get; set; }
    }
}
