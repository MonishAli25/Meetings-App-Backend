namespace Meetings_App_Backend.Models
{
    public class AddTeamRequest
    {
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public List<AddAttendeeRequest> Members { get; set; }
    }
}
