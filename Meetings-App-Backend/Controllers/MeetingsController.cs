using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meetings_App_Backend.Data;
using Meetings_App_Backend.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Meetings_App_Backend.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Meetings_App_Backend.AutoMapper;
namespace Meetings_App_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MeetingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        public MeetingsController(ApplicationDbContext context,IUserService userService,IMapper mapper,UserManager<User> userManager)
        {
            _context = context;
            _userService = userService;
            _mapper = mapper;
            _userManager = userManager;
        }
        [HttpPost]
        [Authorize]
        //public async Task<IActionResult> CreateMeetings([FromBody] MeetingRequest request)
        //{
        //    var userId = _userService.GetUserIdFromClaims(User);



        //    if (request.Attendees.All(a => a.UserId != userId))
        //    {
        //        request.Attendees.Add(new AttendeeModel
        //        {
        //            UserId = userId,
        //            Email = User.FindFirstValue("Email") // or use a claim that stores email
        //        });
        //    }
        //    // Create the new meeting entity
        //    var meeting = new Meetings
        //    {
        //        Name = request.Name,
        //        Description = request.Description,
        //        Date = request.Date,
        //        StartTime = new DateTime(request.Date.Year, request.Date.Month, request.Date.Day, request.StartTime.Hours, request.StartTime.Minutes, 0),
        //        EndTime = new DateTime(request.Date.Year, request.Date.Month, request.Date.Day, request.EndTime.Hours, request.EndTime.Minutes, 0),
        //        // Add the attendees as MeetingAttendees
        //        MeetingAttendees = request.Attendees.Select(a => new MeetingAttendee
        //        {
        //            UserId = a.UserId

        //        }).ToList()
        //    };

        //    // Add the meeting and its attendees to the database
        //    _context.Meetings.Add(meeting);
        //    await _context.SaveChangesAsync();

        //    // Prepare the response model
        //    var response = new MeetingResponse
        //    {
        //        Id = meeting.Id,
        //        Name = meeting.Name,
        //        Description = meeting.Description,
        //        Date = meeting.Date,
        //        StartTime = new TimeModel { Hours = meeting.StartTime.Hour, Minutes = meeting.StartTime.Minute },
        //        EndTime = new TimeModel { Hours = meeting.EndTime.Hour, Minutes = meeting.EndTime.Minute },
        //        Attendees = meeting.MeetingAttendees.Select(a => new AttendeeModel
        //        {
        //            UserId = a.UserId,
        //            Email = a.User.Email
        //        }).ToList()
        //    };

        //    return CreatedAtAction(nameof(CreateMeetings), new { id = meeting.Id }, response);

        //}

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> PostMeeting([FromBody] MeetingRequest request)
        {
            // Ensure the logged-in user is automatically added as an attendee
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID
            var loggedInUser = await _context.Users.FindAsync(userId);

            if (loggedInUser == null)
            {
                return Unauthorized("Logged-in user is not found.");
            }

            var meeting = new Meetings
            {
                Name = request.Name,
                Description = request.Description,
                Date = request.Date,
                //StartTime = new DateTime(request.Date.Year, request.Date.Month, request.Date.Day, request.StartTime.Hours, request.StartTime.Minutes, 0),
                // EndTime = new DateTime(request.Date.Year, request.Date.Month, request.Date.Day, request.EndTime.Hours, request.EndTime.Minutes, 0),


                 StartTime = request.StartTime, // Use only the time portion
                EndTime = request.EndTime,
            };

            meeting.MeetingAttendees = new List<MeetingAttendee>();

            // Add the logged-in user as an attendee
            meeting.MeetingAttendees.Add(new MeetingAttendee
            {
                UserId = loggedInUser.Id, // Add the logged-in user to the meeting
                Meeting = meeting
            });


            // Add other attendees (if any)
            foreach (var attendee in request.Attendees)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == attendee.Email);
                if (user == null)
                {
                    return BadRequest($"User with email {attendee.Email} is not registered.");
                }

                meeting.MeetingAttendees.Add(new MeetingAttendee
                {
                    UserId = user.Id, // Assign the UserId from the found user
                    Meeting = meeting
                });
            }

            // Add the meeting to the database
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Return the newly created meeting details
            var meetingResponse = new MeetingResponse
            {
                Id = meeting.Id,
                Name = meeting.Name,
                Description = meeting.Description,
                Date = meeting.Date,
                StartTime = meeting.StartTime,
        
                EndTime = meeting.EndTime,


                Attendees = await GetAttendeesForMeeting(meeting) // Fetch attendees with their emails
            };

            return Ok(meetingResponse);
        }

        private async Task<List<AttendeeModel>> GetAttendeesForMeeting(Meetings meeting)
        {
            var attendees = new List<AttendeeModel>();

            foreach (var attendee in meeting.MeetingAttendees)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == attendee.UserId);
                if (user != null)
                {
                    attendees.Add(new AttendeeModel
                    {
                        UserId = user.Id,
                        Email = user.Email
                    });
                }
            }

            return attendees;
        }

        [Authorize]
        [HttpGet]
        //public async Task<IActionResult> GetMeetings()
        //{
        //    //var meetings = await _context.Meetings
        //    //    .Include(m => m.MeetingAttendees)
        //    //    .ThenInclude(ma => ma.User)
        //    //    .ToListAsync();

        //    //return Ok(meetings);
        //    var meetings = await _context.Meetings
        //.Include(m => m.MeetingAttendees)
        //.ThenInclude(ma => ma.User)
        //.ToListAsync();

        //    var response = _mapper.Map<List<MeetingResponse>>(meetings);

        //    return Ok(response);
        //}
        public async Task<IActionResult> GetMeetings()
        {
            // Get the current user's ID
            // var currentUserId = _userService.GetCurrentUserId();  // Assuming you have a method to get the current user ID
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized(new { Message = "User is not authenticated" });
            }

            // Retrieve the meetings where the logged-in user is an attendee
            var meetings = await _context.Meetings
                .Where(m => m.MeetingAttendees.Any(ma => ma.UserId == currentUserId))  // Filter by the logged-in user
                .Include(m => m.MeetingAttendees)
                .ThenInclude(ma => ma.User)
                .ToListAsync();

            // Map the meetings to the response model
            var response = _mapper.Map<List<MeetingResponse>>(meetings);

            return Ok(response);
        }

        //[HttpPatch("{meetingId}/add-attendee")]
        //public async Task<IActionResult> AddAttendee(int meetingId, [FromBody] AddAttendeeDto attendeeDto)
        //{
        //    // Check if the user exists in the database
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == attendeeDto.Email);
        //    if (user == null)
        //    {
        //        return BadRequest("User not registered.");
        //    }

        //    // Check if the meeting exists
        //    var meeting = await _context.Meetings.FindAsync(meetingId);
        //    if (meeting == null)
        //    {
        //        return NotFound("Meeting not found.");
        //    }

        //    // Check if the user is already an attendee
        //    var existingAttendee = await _context.MeetingAttendees
        //        .AnyAsync(ma => ma.MeetingId == meetingId && ma.UserId == user.Id);

        //    if (existingAttendee)
        //    {
        //        return BadRequest("User is already an attendee.");
        //    }

        //    // Add the user as an attendee
        //    var meetingAttendee = new MeetingAttendee
        //    {
        //        MeetingId = meetingId,
        //        UserId = user.Id
        //    };

        //    _context.MeetingAttendees.Add(meetingAttendee);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { message = "User added as attendee." });
        //}

        //  [HttpPatch("{id}")]
        //public async Task<IActionResult> PatchMeeting(int id, [FromQuery] string action, [FromQuery] int? userId, [FromQuery] string email)
        //{
        //    // Check if the meeting exists
        //    var meeting = await _context.Meetings
        //        .Include(m => m.MeetingAttendees)
        //        .ThenInclude(ma => ma.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (meeting == null)
        //    {
        //        return NotFound(new { Message = "Meeting not found" });
        //    }

        //    // Handle the add_attendee action
        //    if (action.Equals("add_attendee", StringComparison.OrdinalIgnoreCase) && userId.HasValue && !string.IsNullOrEmpty(email))
        //    {
        //        // Check if the user already exists
        //        var user = await _userService.GetUserIdFromClaims(userId.Value.ToString());

        //        if (user == null)
        //        {
        //            return BadRequest(new { Message = "User not found" });
        //        }

        //        // Check if the user is already an attendee
        //        var existingAttendee = meeting.MeetingAttendees.FirstOrDefault(ma => ma.UserId == user.Id);
        //        if (existingAttendee != null)
        //        {
        //            return BadRequest(new { Message = "User is already an attendee of this meeting" });
        //        }

        //        // Add the user to the meeting
        //        meeting.MeetingAttendees.Add(new MeetingAttendee
        //        {
        //            UserId = user.Id,
        //            MeetingId = meeting.Id
        //        });

        //        await _context.SaveChangesAsync();

        //        // Return the updated meeting
        //        var updatedMeeting = await _context.Meetings
        //            .Include(m => m.MeetingAttendees)
        //            .ThenInclude(ma => ma.User)
        //            .FirstOrDefaultAsync(m => m.Id == id);

        //        var response = _mapper.Map<MeetingResponse>(updatedMeeting);
        //        return Ok(response);
        //    }

        //    // Handle the remove_attendee action (assumes the logged-in user is the one to be removed)
        //    if (action.Equals("remove_attendee", StringComparison.OrdinalIgnoreCase))
        //    {
        //   //     var currentUserId = _userService.GetUserId(User); // Get the current logged-in user's ID
        //        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        //        if (string.IsNullOrEmpty(currentUserId))
        //        {
        //            return Unauthorized(new { Message = "User is not authenticated" });
        //        }

        //        var attendeeToRemove = meeting.MeetingAttendees.FirstOrDefault(ma => ma.UserId.ToString() == currentUserId);
        //        if (attendeeToRemove == null)
        //        {
        //            return BadRequest(new { Message = "User is not an attendee of this meeting" });
        //        }

        //        // Remove the attendee
        //        meeting.MeetingAttendees.Remove(attendeeToRemove);
        //        await _context.SaveChangesAsync();

        //        // Return the updated meeting
        //        var updatedMeeting = await _context.Meetings
        //            .Include(m => m.MeetingAttendees)
        //            .ThenInclude(ma => ma.User)
        //            .FirstOrDefaultAsync(m => m.Id == id);

        //        var response = _mapper.Map<MeetingResponse>(updatedMeeting);
        //        return Ok(response);
        //    }

        //    // If the action is invalid
        //    return BadRequest(new { Message = "Invalid action specified" });
        //}

        //[HttpPatch("{id}")]
        //public async Task<IActionResult> PatchMeeting(int id, [FromQuery] string action, [FromQuery] int? userId, [FromQuery] string email)
        //{
        //    // Check if the meeting exists
        //    var meeting = await _context.Meetings
        //        .Include(m => m.MeetingAttendees)
        //        .ThenInclude(ma => ma.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);

        //    if (meeting == null)
        //    {
        //        return NotFound(new { Message = "Meeting not found" });
        //    }

        //    // Handle the add_attendee action
        //    if (action.Equals("add_attendee", StringComparison.OrdinalIgnoreCase) && userId.HasValue && !string.IsNullOrEmpty(email))
        //    {
        //        // Use IUserService to find the user
        //        var user = await _userService.GetUserByIdAsync(userId.Value);
        //        if (user == null)
        //        {
        //            return BadRequest(new { Message = "User not found" });
        //        }

        //        // Check if the user is already an attendee
        //        var existingAttendee = meeting.MeetingAttendees.FirstOrDefault(ma => ma.UserId == user.Id);
        //        if (existingAttendee != null)
        //        {
        //            return BadRequest(new { Message = "User is already an attendee of this meeting" });
        //        }

        //        // Add the user to the meeting
        //        meeting.MeetingAttendees.Add(new MeetingAttendee
        //        {
        //            UserId = user.Id,
        //            MeetingId = meeting.Id
        //        });

        //        await _context.SaveChangesAsync();

        //        // Return the updated meeting
        //        var updatedMeeting = await _context.Meetings
        //            .Include(m => m.MeetingAttendees)
        //            .ThenInclude(ma => ma.User)
        //            .FirstOrDefaultAsync(m => m.Id == id);

        //        var response = _mapper.Map<MeetingResponse>(updatedMeeting);
        //        return Ok(response);
        //    }

        //    // Handle the remove_attendee action (assumes the logged-in user is the one to be removed)
        //    if (action.Equals("remove_attendee", StringComparison.OrdinalIgnoreCase))
        //    {
        //        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //      //  var currentUserId = _userService.GetCurrentUserId();  // Assuming GetCurrentUserId method is in IUserService
        //        if (string.IsNullOrEmpty(currentUserId))
        //        {
        //            return Unauthorized(new { Message = "User is not authenticated" });
        //        }

        //        // Convert currentUserId to integer if necessary
        //        if (int.TryParse(currentUserId, out int userIdToRemove))
        //        {
        //            var attendeeToRemove = meeting.MeetingAttendees.FirstOrDefault(ma => ma.UserId == userIdToRemove);
        //            if (attendeeToRemove == null)
        //            {
        //                return BadRequest(new { Message = "User is not an attendee of this meeting" });
        //            }

        //            // Remove the attendee
        //            meeting.MeetingAttendees.Remove(attendeeToRemove);
        //            await _context.SaveChangesAsync();

        //            // Return the updated meeting
        //            var updatedMeeting = await _context.Meetings
        //                .Include(m => m.MeetingAttendees)
        //                .ThenInclude(ma => ma.User)
        //                .FirstOrDefaultAsync(m => m.Id == id);

        //            var response = _mapper.Map<MeetingResponse>(updatedMeeting);
        //            return Ok(response);
        //        }

        //        return BadRequest(new { Message = "Failed to parse the current user ID" });
        //    }

        //    // If the action is invalid
        //    return BadRequest(new { Message = "Invalid action specified" });
        //}


        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchMeeting(int id, [FromQuery] string action, [FromQuery] string? userId, [FromQuery] string email)
        {
            // Check if the meeting exists
            var meeting = await _context.Meetings
                .Include(m => m.MeetingAttendees)
                .ThenInclude(ma => ma.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (meeting == null)
            {
                return NotFound(new { Message = "Meeting not found" });
            }

            // Handle the add_attendee action
            if (action.Equals("add_attendee", StringComparison.OrdinalIgnoreCase) && userId!=null && !string.IsNullOrEmpty(email))
            {
                // Check if the user already exists
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return BadRequest(new { Message = "User not found" });
                }

                // Check if the user is already an attendee
                var existingAttendee = meeting.MeetingAttendees.FirstOrDefault(ma => ma.UserId == user.Id);
                if (existingAttendee != null)
                {
                    return BadRequest(new { Message = "User is already an attendee of this meeting" });
                }

                // Add the user to the meeting
                meeting.MeetingAttendees.Add(new MeetingAttendee
                {
                    UserId = user.Id,
                    MeetingId = meeting.Id
                });

                await _context.SaveChangesAsync();

                // Return the updated meeting
                var updatedMeeting = await _context.Meetings
                    .Include(m => m.MeetingAttendees)
                    .ThenInclude(ma => ma.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                var response = _mapper.Map<MeetingResponse>(updatedMeeting);
                return Ok(response);
            }

            // Handle the remove_attendee action (assumes the logged-in user is the one to be removed)
            if (action.Equals("remove_attendee", StringComparison.OrdinalIgnoreCase))
            {
                var currentUserId = _userManager.GetUserId(User); // Get the current logged-in user's ID
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized(new { Message = "User is not authenticated" });
                }

                var attendeeToRemove = meeting.MeetingAttendees.FirstOrDefault(ma => ma.UserId.ToString() == currentUserId);
                if (attendeeToRemove == null)
                {
                    return BadRequest(new { Message = "User is not an attendee of this meeting" });
                }

                // Remove the attendee
                meeting.MeetingAttendees.Remove(attendeeToRemove);
                await _context.SaveChangesAsync();

                // Return the updated meeting
                var updatedMeeting = await _context.Meetings
                    .Include(m => m.MeetingAttendees)
                    .ThenInclude(ma => ma.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                var response = _mapper.Map<MeetingResponse>(updatedMeeting);
                return Ok(response);
            }

            // If the action is invalid
            return BadRequest(new { Message = "Invalid action specified" });
        }

    }
    public class AddAttendeeDto
    {
        public string Email { get; set; }
    }
}
