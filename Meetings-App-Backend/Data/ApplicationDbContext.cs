using Microsoft.EntityFrameworkCore;
using Meetings_App_Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace Meetings_App_Backend.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Meetings> Meetings { get; set; }

        public DbSet<Team> Teams { get; set; }
        public DbSet<MeetingAttendee> MeetingAttendees { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for MeetingAttendee
            modelBuilder.Entity<MeetingAttendee>()
                .HasKey(ma => new { ma.UserId, ma.MeetingId });

            modelBuilder.Entity<MeetingAttendee>()
                .HasOne(ma => ma.User)
                .WithMany(u => u.MeetingAttendees)
                .HasForeignKey(ma => ma.UserId);

            modelBuilder.Entity<MeetingAttendee>()
                .HasOne(ma => ma.Meeting)
                .WithMany(m => m.MeetingAttendees)
                .HasForeignKey(ma => ma.MeetingId);

            // Configure composite key for TeamMember
            modelBuilder.Entity<TeamMember>()
                .HasKey(tm => new { tm.UserId, tm.TeamId });

            // Define relationships for TeamMember
            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.Teams)
                .HasForeignKey(tm => tm.UserId);

            modelBuilder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId);

            var readerRoleId = "a71a55d6-99d7-4123-b4e0-1218ecb90e3e";
            var writerRoleId = "c309fa92-2123-47be-b397-a1c77adb502c";

            var roles = new List<IdentityRole>
        {
            new IdentityRole
            {
                Id = readerRoleId,
                ConcurrencyStamp = readerRoleId,
                Name = "Reader",
                NormalizedName = "Reader".ToUpper()
            },
            new IdentityRole
            {
                Id = writerRoleId,
                ConcurrencyStamp = writerRoleId,
                Name = "Writer",
                NormalizedName = "Writer".ToUpper()
            }
        };

            modelBuilder.Entity<IdentityRole>().HasData(roles);
        }

    }
}
