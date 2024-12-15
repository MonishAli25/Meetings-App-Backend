using Microsoft.AspNetCore.Identity;

namespace Meetings_App_Backend.Models
{
    public class User : IdentityUser
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Navigation property to the meetings the user is attending
        public ICollection<MeetingAttendee> MeetingAttendees { get; set; }

        public ICollection<TeamMember> Teams { get; set; }
    }
}
