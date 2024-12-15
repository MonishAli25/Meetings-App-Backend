namespace Meetings_App_Backend.Models
{
    public class MeetingRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public List<AddAttendeeRequest> Attendees
        {
            get; set;
        }
    }
}
