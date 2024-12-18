using Microsoft.AspNetCore.Identity;

namespace Meetings_App_Backend.Models
{
    public class User : IdentityUser
    {
        

        // Navigation property to the meetings the user is attending
        public ICollection<MeetingAttendee> MeetingAttendees { get; set; }

        public ICollection<TeamMember> Teams { get; set; }
    }
}
