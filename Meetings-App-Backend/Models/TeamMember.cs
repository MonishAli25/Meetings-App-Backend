namespace Meetings_App_Backend.Models
{
    public class TeamMember
    {
        public int TeamId { get; set; }
        public Team Team { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
    }
}
