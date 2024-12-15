namespace Meetings_App_Backend.Models
{
    public class MeetingAttendee
    {
        public int MeetingId { get; set; }
        public Meetings Meeting { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
    }
}
