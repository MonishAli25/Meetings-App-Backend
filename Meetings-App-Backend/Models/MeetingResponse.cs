namespace Meetings_App_Backend.Models
{
    public class MeetingResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public List<AttendeeModel> Attendees { get; set; }
    }

   

}
