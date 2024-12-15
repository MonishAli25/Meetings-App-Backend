using System;
using System.Collections.Generic;

namespace Meetings_App_Backend.Models
{
    public class Meetings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Navigation property to the attendees
        public ICollection<MeetingAttendee>  Attendees { get; set; }
    }
}
