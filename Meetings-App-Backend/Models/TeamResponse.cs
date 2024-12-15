namespace Meetings_App_Backend.Models
{
    public class TeamResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public List<UserResponse> Members { get; set; }
    }
}
