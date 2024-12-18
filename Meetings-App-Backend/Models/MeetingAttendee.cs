using System.Text.Json.Serialization;

namespace Meetings_App_Backend.Models
{
    public class MeetingAttendee
    {
        public  int MeetingId { get; set; }

        [JsonIgnore]
        public Meetings Meeting { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
