using Telegram.Bot.Types;


namespace VitalityProgramOnline.Apps.Telegram.Interfaces
{
    public interface IMessageHandler
    {
        public Task HandlePollingErrorAsync(Exception exception, CancellationToken cancellationToken);
        public Task HandleUpdateAsync(Update update, CancellationToken cancellationToken);
        public bool IsAdmin(long userId);
    }
}
