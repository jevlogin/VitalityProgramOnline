using VitalityProgramOnline.Models.User;

namespace VitalityProgramOnline.Helper
{
    public static class Extentions
    {
        public static DateTime ChangeDate(this DateTime dateTime, TimeSpan timeSpan)
        {
            return new DateTime(
               dateTime.Year,
                dateTime.Month,
                dateTime.Day,
                timeSpan.Hours,
                timeSpan.Minutes,
                timeSpan.Seconds);
        }

        public static ApplicationUser UpdateUser(this ApplicationUser existingUser, ApplicationUser newUser)
        {
            if (newUser.Email != null)
            {
                existingUser.Email = newUser.Email;
            }

            if(newUser.EmailConfirmed != existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = newUser.EmailConfirmed;
            }

            if (newUser.PasswordHash != null)
            {
                existingUser.PasswordHash = newUser.PasswordHash;
            }

            if (newUser.PhoneNumber != null)
            {
                existingUser.PhoneNumber = newUser.PhoneNumber;
            }

            if (newUser.UserName != null)
            {
                existingUser.UserName = newUser.UserName;
            }

            if (newUser.IsAdmin != null)
            {
                existingUser.IsAdmin = newUser.IsAdmin;
            }

            if (newUser.FirstName != null)
            {
                existingUser.FirstName = newUser.FirstName;
            }

            if (newUser.LastName != null)
            {
                existingUser.LastName = newUser.LastName;
            }

            if (newUser.TelegramUsername != null)
            {
                existingUser.TelegramUsername = newUser.TelegramUsername;
            }

            if (newUser.InstagramUsername != null)
            {
                existingUser.InstagramUsername = newUser.InstagramUsername;
            }

            if (newUser.PhoneNumberTwo != null)
            {
                existingUser.PhoneNumberTwo = newUser.PhoneNumberTwo;
            }

            if (newUser.Height != null)
            {
                existingUser.Height = newUser.Height;
            }

            if (newUser.Weight != null)
            {
                existingUser.Weight = newUser.Weight;
            }

            if (newUser.DateOfBirth != null)
            {
                existingUser.DateOfBirth = newUser.DateOfBirth;
            }

            if (newUser.Diet != null)
            {
                existingUser.Diet = newUser.Diet;
            }

            if (newUser.IsSmoker != null)
            {
                existingUser.IsSmoker = newUser.IsSmoker;
            }

            if (newUser.IsAllergic != null)
            {
                existingUser.IsAllergic = newUser.IsAllergic;
            }

            if (newUser.HasMedicalConditions != null)
            {
                existingUser.HasMedicalConditions = newUser.HasMedicalConditions;
            }

            if (newUser.MedicalConditions != null)
            {
                existingUser.MedicalConditions = newUser.MedicalConditions;
            }

            if (newUser.FitnessGoals != null)
            {
                existingUser.FitnessGoals = newUser.FitnessGoals;
            }

            return existingUser;
        }
    }
}
