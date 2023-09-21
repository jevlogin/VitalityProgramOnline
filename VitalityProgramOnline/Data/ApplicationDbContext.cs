using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VitalityProgramOnline.Models.Feedback;
using VitalityProgramOnline.Models.FoodDiary;
using VitalityProgramOnline.Models.User;
using VitalityProgramOnline.Models.User.Settings;


namespace VitalityProgramOnline.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
        public DbSet<ProgressUsers> ProgressUsers { get; set; }
        public DbSet<UserBotSettings> UserBotSettings { get; set; }
        public DbSet<FeedbackResponse> FeedbackResponses { get; set; }
        public DbSet<FoodDiaryEntry> FoodDiary { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }
}