using System.ComponentModel.DataAnnotations;

namespace Meetings_App_Backend.Models
{
    public class MeetingRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [Required]
        [DataType(DataType.Time)]
        public TimeOnly StartTime { get; set; }
        [Required]
        [DataType(DataType.Time)]
        public TimeOnly EndTime { get; set; }
        public List<AttendeeModel> Attendees { get; set; }
    }

    //public class TimeModel
    //{
    //    public int Hours { get; set; }
    //    public int Minutes { get; set; }
    //}

    public class AttendeeModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
