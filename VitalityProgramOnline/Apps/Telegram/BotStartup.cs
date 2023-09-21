using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Telegram.Bot;
using Telegram.Bot.Polling;
using VitalityProgramOnline.Apps.Telegram.Controler;
using VitalityProgramOnline.Data;
using VitalityProgramOnline.Models.User;

namespace VitalityProgramOnline.Apps.Telegram
{
    public class BotStartup
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public BotStartup(IConfiguration configuration, ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var appConfig = _configuration.Get<AppConfig>();
            if (appConfig == null)
                return;

            var botClient = new TelegramBotClient(appConfig.BotKeyDebug);

            var databaseService = new DatabaseService(_dbContext, appConfig, _userManager);
            await databaseService.MigrateAsync();

            var adminList = await databaseService.LoadAdminListAsync();
            var userList = await databaseService.LoadUserListAsync();

            var adminMessageHandler = new AdminMessageHandler(botClient, databaseService, adminList, userList);
            var userMessageHandler = new UserMessageHandler(botClient, databaseService, adminList, userList);

            var updateDispatcher = new UpdateDispatcher();
            updateDispatcher.AddHandler(adminMessageHandler);
            updateDispatcher.AddHandler(userMessageHandler);

            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            ReceiverOptions receiverOptions = new ReceiverOptions();

            botClient.StartReceiving(
                updateDispatcher,
                receiverOptions: receiverOptions,
                cancellationToken: cancellationTokenSource.Token);

            var me = await botClient.GetMeAsync();

            await Console.Out.WriteLineAsync($"Начало работы бота {me.Username}");
        }
    }
}
