using Microsoft.AspNetCore.Identity;
using VitalityProgramOnline.Helper;
using VitalityProgramOnline.Models.FoodDiary;


namespace VitalityProgramOnline.Models.User
{
    public class ApplicationUser : IdentityUser
    {
        public long? UserId { get; set; }
        public bool? IsAdmin { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? TelegramUsername { get; set; }
        public string? InstagramUsername { get; set; }
        public string? PhoneNumberTwo { get; set; }
        public int? Height { get; set; }
        public double? Weight { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DietType? Diet { get; set; }
        public bool? IsSmoker { get; set; }
        public bool? IsAllergic { get; set; }
        public string? Allergic { get; set; }
        public bool? HasMedicalConditions { get; set; }
        public string? MedicalConditions { get; set; }
        public string? FitnessGoals { get; set; }

        public ICollection<FoodDiaryEntry> FoodDiaryEntries { get; set; }
    }
}
