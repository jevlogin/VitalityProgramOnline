using Telegram.Bot;
using Telegram.Bot.Types;
using VitalityProgramOnline.Apps.Telegram.Interfaces;


namespace VitalityProgramOnline.Apps.Telegram.Controler
{
    public abstract class MessageHandler : IMessageHandler
    {
        protected readonly ITelegramBotClient _botClient;

        public MessageHandler(ITelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public abstract bool IsAdmin(long userId);

        public abstract Task HandlePollingErrorAsync(Exception exception, CancellationToken cancellationToken);

        public abstract Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
    }
}
