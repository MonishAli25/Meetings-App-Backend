using Meetings_App_Backend.Data;
using Meetings_App_Backend.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Meetings_App_Backend.Services
{
    public class UserService:IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly UserManager<User> userManager;

        public UserService(ApplicationDbContext context, IConfiguration configuration, IPasswordHasher<User> passwordHasher, UserManager<User> userManager)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = passwordHasher;
            this.userManager = userManager;
        }

        public async Task<List<UserResponse>> GetUsersAsync()
        {
            return await _context.Users
                .Select(u => new UserResponse { Id = u.Id, UserName = u.UserName, Email = u.Email })
                .ToListAsync();
        }


        public async Task<User> RegisterAsync(RegisterRequest model)
        {
            string[] roles = new string[] { "Reader", "Writer" }; 

            var userExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (userExists)
                throw new Exception("User with this email already exists");

            var user = new User
            {
                UserName = model.Name,
                Email = model.Email,
            };

            var identityResult = await userManager.CreateAsync(user,model.Password);

            if (identityResult.Succeeded)
            {
                identityResult = await userManager.AddToRolesAsync(user, roles);
            }



            if (identityResult.Succeeded)
            {


                return user;
            }
            else
            {
                throw new Exception("not registered");
            }

           
        }

        public async Task<User> AuthenticateAsync(LoginRequest model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null) return null;

            var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, model.Password);
            return result == PasswordVerificationResult.Failed ? null : user;
        }

        public async Task<bool> UserExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email); // Asynchronous query
        }

        public string GenerateJwtToken(User user)
        {

            var secretKey = _configuration["Jwt:Key"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new InvalidOperationException("JWT Secret key is missing from configuration.");
            }


            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, "Reader"),
                new Claim(ClaimTypes.Role, "Writer"),
        };

            



            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetUserIdFromClaims(ClaimsPrincipal user) => (user.FindFirstValue(ClaimTypes.NameIdentifier));




       

        public async Task<Team> CreateTeamAsync(AddTeamRequest model)
        {
            var team = new Team
            {
                Name = model.Name,
                Description = model.Description
            };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team;
        }

        public async Task<Team> AddMemberToTeamAsync(int teamId, string userId)
        {
            var team = await _context.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == teamId);
            if (team != null)
            {
                team.Members.Add(new TeamMember { UserId = userId });
                await _context.SaveChangesAsync();
            }
            return team;
        }

        public async Task<Team> RemoveMemberFromTeamAsync(int teamId, string userId)
        {
            var team = await _context.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.Id == teamId);
            if (team != null)
            {
                var member = team.Members.FirstOrDefault(m => m.UserId == userId);
                if (member != null)
                {
                    team.Members.Remove(member);
                    await _context.SaveChangesAsync();
                }
            }
            return team;
        }
    }
}

