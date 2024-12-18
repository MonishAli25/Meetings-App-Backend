using Meetings_App_Backend.Models;
using System.Security.Claims;

namespace Meetings_App_Backend.Services
{
    public interface IUserService
    {
        Task<List<UserResponse>> GetUsersAsync();
        Task<User> RegisterAsync(RegisterRequest model);
        Task<User> AuthenticateAsync(LoginRequest model);
        Task<bool> UserExistsAsync(string email);
        string GenerateJwtToken(User user);
        string GetUserIdFromClaims(ClaimsPrincipal user);


      //  Task<List<Team>> GetTeamsByUserAsync();
        Task<Team> CreateTeamAsync(AddTeamRequest model);
        Task<Team> AddMemberToTeamAsync(int teamId, string userId);
        Task<Team> RemoveMemberFromTeamAsync(int teamId, string userId);



        
        

    }
}
