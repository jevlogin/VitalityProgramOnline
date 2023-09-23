using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VitalityProgramOnline.Data;
using VitalityProgramOnline.Helper;
using VitalityProgramOnline.Models.Feedback;
using VitalityProgramOnline.Models.FoodDiary;
using VitalityProgramOnline.Models.User;
using VitalityProgramOnline.Models.User.Settings;


namespace VitalityProgramOnline.Apps.Telegram
{
    public class DatabaseService
    {
        #region Fields

        private ApplicationDbContext _dbContext;
        private readonly AppConfig _config;
        private readonly UserManager<ApplicationUser> _userManager;

        #endregion


        #region ClassLifeCycles

        public DatabaseService(ApplicationDbContext dbContext, AppConfig appConfig, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _config = appConfig;
            _userManager = userManager;
        }

        #endregion

        #region MigrateAsync

        internal async Task MigrateAsync()
        {
            await AddFirstAdminAsync();
        }

        private async Task AddFirstAdminAsync()
        {
            var usernameAdmin = $"VPO_Consultant_{_config.FirstAdminId}";
            var firstAdmin = await _dbContext.ApplicationUser.FirstOrDefaultAsync(user => user.UserId!.Value == _config.FirstAdminId);

            if (firstAdmin == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserId = _config.FirstAdminId,
                    Email = $"{usernameAdmin}@vitalityprogram.online",
                    UserName = usernameAdmin,
                    IsAdmin = true,
                    FirstName = usernameAdmin
                };

                var password = "pass.WD!23";
                var result = await _userManager.CreateAsync(newAdmin, password);

                if (result.Succeeded)
                {
                    await Console.Out.WriteLineAsync($"Первый администратор успешно создан.");
                }
                else
                {
                    await Console.Out.WriteLineAsync($"Не удалось создать первого администратора.");
                    foreach (var error in result.Errors)
                    {
                        await Console.Out.WriteLineAsync(error.Description);
                    }
                }
            }
        }

        #endregion


        #region LoadAdminListAsync

        internal async Task<Dictionary<long, ApplicationUser>> LoadAdminListAsync()
        {
            var adminList = await _dbContext.ApplicationUser
                .Where(admin => admin.IsAdmin == true)
                .Where(admin => admin.UserId != null)
                .ToDictionaryAsync(admin => admin.UserId!.Value, admin => admin);

            return adminList;
        }


        internal async Task AddAdminAsync(ApplicationUser admin)
        {
            var adminInServer = await _dbContext.ApplicationUser.FindAsync(admin.UserId);
            if (adminInServer == null)
            {
                await _dbContext.ApplicationUser.AddAsync(admin);
            }
            else
            {
                _dbContext.Entry(adminInServer).CurrentValues.SetValues(admin);
                _dbContext.Entry(adminInServer).Property(u => u.Id).IsModified = false;
            }

            await _dbContext.SaveChangesAsync();
        }

        #endregion


        #region LoadUserListAsync

        internal async Task<Dictionary<long, ApplicationUser>> LoadUserListAsync()
        {
            var userList = await _dbContext.ApplicationUser
                .Where(admin => admin.UserId != null)
                .ToDictionaryAsync(admin => admin.UserId!.Value, admin => admin);

            return userList;
        }

        internal async Task<bool> LoadUserAnyAsync(long userId)
        {
            return await _dbContext.ApplicationUser.AnyAsync(u => u.UserId == userId);
        }

        #endregion


        #region LoadProgressUsers

        internal async Task<Dictionary<long, ProgressUsers>> LoadProgressUsersAsync()
        {
            await Console.Out.WriteLineAsync($"Загрузка прогресса пользователей.");
            var progressList = await _dbContext.ProgressUsers.ToDictionaryAsync(progress => progress.UserId, progress => progress);
            return progressList;
        }

        #endregion


        #region UpdateUserProgressAsync

        internal async Task<bool> UpdateUserProgressAsync(ProgressUsers progress)
        {
            try
            {
                var existProgress = await _dbContext.ProgressUsers.FirstOrDefaultAsync(p => p.UserId == progress.UserId);
                if (existProgress == null)
                {
                    await _dbContext.ProgressUsers.AddAsync(progress);
                    await Console.Out.WriteLineAsync($"Был добавлен прогресс пользователя - {DialogData.SUCCESS}");
                }
                else
                {
                    _dbContext.Entry(existProgress).CurrentValues.SetValues(progress);
                    _dbContext.Entry(existProgress).Property(progress => progress.UserId).IsModified = false;

                }

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Обновление прогресса пользователя - {DialogData.FAILED}\n{ex.Message}");
                return false;
            }
        }

        #endregion


        #region ReadUserBotSettings

        internal async Task<EmptyBotSettings> ReadUserBotSettings(long userId, CancellationToken none)
        {
            var userBotSettings = await _dbContext.UserBotSettings.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userBotSettings == null)
            {
                return new EmptyBotSettings();
            }
            return userBotSettings;
        }

        #endregion


        #region AddOrUpdateUserAsync

        internal async Task AddOrUpdateUserAsync(ApplicationUser user)
        {
            var existingUsers = await _dbContext.ApplicationUser.FirstOrDefaultAsync(u => u.UserId == user.UserId);

            if (existingUsers == null)
            {
                //await _dbContext.ApplicationUser.AddAsync(user);
                
                var result = await _userManager.CreateAsync(user, "tempPWD@1");

                if (result.Succeeded)
                {
                    await Console.Out.WriteLineAsync($"Пользователь {user.FirstName} успешно создан.");
                }
                else
                {
                    await Console.Out.WriteLineAsync($"Не удалось создать первого администратора.");
                    foreach (var error in result.Errors)
                    {
                        await Console.Out.WriteLineAsync(error.Description);
                    }
                }
            }
            else
            {
                await _userManager.UpdateAsync(user);

                //_dbContext.Entry(existingUsers).Property(u => u.Id).IsModified = false;
                //_dbContext.Entry(existingUsers).CurrentValues.SetValues(user);
            }

            //await _dbContext.SaveChangesAsync();
        }

        #endregion


        #region AddOrUpdateFeedbackResponseAsync

        internal async Task AddOrUpdateFeedbackResponseAsync(FeedbackResponse feedbackResponse)
        {
            var existingFeedback = await _dbContext.FeedbackResponses.SingleOrDefaultAsync(feedback => feedback.ResponseId == feedbackResponse.ResponseId);
            try
            {
                if (existingFeedback == null)
                {
                    await _dbContext.FeedbackResponses.AddAsync(feedbackResponse);
                    await Console.Out.WriteLineAsync($"Была добавлена новая запись ответов обратной связи, в базу данных");
                }
                else
                {
                    _dbContext.Entry(existingFeedback).CurrentValues.SetValues(feedbackResponse);
                }
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Не получилось добавить запись\n{ex.Message}");
            }
        }

        #endregion


        #region AddOrUpdateBotSettingsAsync

        internal async Task AddOrUpdateBotSettingsAsync(UserBotSettings userBotSettings)
        {
            var existingSettings = await _dbContext.UserBotSettings.SingleOrDefaultAsync(botConfig => botConfig.UserId == userBotSettings.UserId);
            if (existingSettings == null)
            {
                await _dbContext.UserBotSettings.AddAsync(userBotSettings);
                await Console.Out.WriteLineAsync($"Была добавлена новая запись в дневник питания, в базу данных");
            }
            else
            {
                _dbContext.Entry(existingSettings).CurrentValues.SetValues(userBotSettings);
            }

            await _dbContext.SaveChangesAsync();
        }

        #endregion


        #region FoodDiaryAsync

        internal async Task AddOrUpdateFoodDiaryAsync(FoodDiaryEntry foodDiary)
        {
            var existingFoodDiary = await _dbContext.FoodDiary.SingleOrDefaultAsync(x => x.Id == foodDiary.Id && x.UserId == foodDiary.UserId);
            if (existingFoodDiary == null)
            {
                await _dbContext.FoodDiary.AddAsync(foodDiary);
                await Console.Out.WriteLineAsync($"Была добавлена новая запис в дневник питания, в базу данных");
            }
            else
            {
                _dbContext.Entry(existingFoodDiary).CurrentValues.SetValues(foodDiary);
            }

            await _dbContext.SaveChangesAsync();
        }

        internal async Task<IEnumerable<FoodDiaryEntry>> ReadTheFoodDiaryForTheCurrentDay(long id, CancellationToken cancellationToken)
        {
            return await _dbContext.FoodDiary.Where(entry => entry.UserId == id && entry.Date == DateTime.Now.Date).ToListAsync();
        }

        #endregion
    }
}
