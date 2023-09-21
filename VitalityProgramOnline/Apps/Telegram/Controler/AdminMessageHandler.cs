using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using VitalityProgramOnline.Models.User;


namespace VitalityProgramOnline.Apps.Telegram.Controler
{
    public class AdminMessageHandler : MessageHandler
    {
        #region Fields

        private readonly DatabaseService _databaseService;
        private readonly Dictionary<long, ApplicationUser> _userList;
        private Dictionary<long, ApplicationUser> _adminList;

        #endregion


        #region ClassLifeCycles

        public AdminMessageHandler(TelegramBotClient botClient, DatabaseService databaseService,
                                   Dictionary<long, ApplicationUser> adminList, Dictionary<long, ApplicationUser> userList) : base(botClient)
        {
            _databaseService = databaseService;
            _adminList = adminList;
            _userList = userList;
        }

        #endregion


        #region MessageHandler

        #region HandlePollingErrorAsync

        public async override Task HandlePollingErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync($"An error occurred during handling admin message: {exception}");

            if (exception is ApiRequestException apiException)
            {
                await Console.Out.WriteLineAsync($"API error occurred: {apiException.ErrorCode} - {apiException.Message}");
            }
            else
            {
                await Console.Out.WriteLineAsync("An unknown error occurred.");
            }
        }

        #endregion


        #region HandleUpdateAsync

        public override async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is { } message)
                    {
                        switch (message.Type)
                        {
                            case MessageType.Unknown:
                                break;
                            case MessageType.Text:
                                if (message.Text is { } text)
                                {
                                    if (text.StartsWith('/'))
                                    {
                                        await HandleCommandAsync(message, cancellationToken);
                                    }
                                    else
                                    {
                                        await HandleTextAsync(message, cancellationToken);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case UpdateType.CallbackQuery:

                    break;
            }
        }

        #endregion


        #region IsAdmin

        public override bool IsAdmin(long userId)
        {
            return _adminList.ContainsKey(userId);
        }

        #endregion

        #endregion


        #region Methods

        #region HandleCommandAsync

        private async Task HandleCommandAsync(Message message, CancellationToken cancellationToken)
        {
            if (message.Text is not { } text) return;

            var command = text.Split(' ')[0].ToLower();
            var args = text.Split(' ').Skip(1).ToArray();

            switch (command)
            {
                case "/start":
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Привет Админ! Ты управляешь этим чатом.");
                    break;
                case "/help":
                    await _botClient.SendTextMessageAsync(
                        message.Chat.Id,
                         "Вы можете использовать следующие команды:\n" +
                        "/start - Начать общение\n" +
                        "/help - Помощь\n" +
                        "/status - Показать статус бота\n");
                    break;
                case "/status":
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Бот исправно работает и слушает все сообщения.");
                    break;
                case "/addadmin":
                    await HandleAddAdminCommandAsync(message, args, cancellationToken);
                    break;
                default:
                    await _botClient.SendTextMessageAsync(message.Chat.Id, "Простите, но я не понимаю данной команды.", cancellationToken: cancellationToken);
                    break;
            }
        }

        #endregion


        #region HandleAddAdminCommandAsync

        private async Task HandleAddAdminCommandAsync(Message message, string[] args, CancellationToken cancellationToken)
        {
            if (message.From?.Id is { } id)
            {
                if (IsAdmin(id))
                {
                    if (args.Length > 0)
                    {
                        long userId = long.Parse(args[0]);

                        if (_adminList.TryGetValue(userId, out var admin))
                        {
                            var msgInfo = $"Такой администратор уже есть";
                            await Console.Out.WriteLineAsync(msgInfo);
                            await _botClient.SendTextMessageAsync(message.Chat.Id, msgInfo, cancellationToken: cancellationToken);
                            return;
                        }

                        _adminList[userId] = new ApplicationUser { UserId = userId, IsAdmin = true, FirstName = $"Admin_{userId}" };

                        await _databaseService.AddAdminAsync(_adminList[userId]);
                    }
                    else
                    {
                        await _botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            "Пожалуйста введите корректные данные",
                            cancellationToken: cancellationToken);
                    }
                }
            }
        }

        #endregion


        #region HandleTextAsync

        private async Task HandleTextAsync(Message message, CancellationToken cancellationToken)
        {
            switch (message.Type)
            {
                case MessageType.Unknown:
                    break;
                case MessageType.Text:
                    if (message.ReplyToMessage is { } replyToMessage)
                    {
                        if (replyToMessage.ForwardFrom is { } forwardFrom && forwardFrom.Id != _botClient.BotId)
                        {
                            var userId = forwardFrom.Id;
                            var adminName = message.From.FirstName;

                            if (message.Text is { } text)
                            {
                                var msgText = $"<b>{adminName}</b>: <i>{text}</i>";

                                try
                                {
                                    await _botClient.SendTextMessageAsync(userId, msgText,
                                                                            parseMode: ParseMode.Html, replyToMessageId: replyToMessage.MessageId,
                                                                            cancellationToken: cancellationToken);
                                }
                                catch (ApiRequestException ex)
                                {
                                    await Console.Out.WriteLineAsync($"По не известной причине, блок метода - 'HandleTextAsync', не отработал.\n{ex.Message}");

                                    await _botClient.SendTextMessageAsync(userId, msgText, parseMode: ParseMode.Html,
                                                                            cancellationToken: cancellationToken);
                                }
                            }
                        }
                    }

                    await _botClient.SendTextMessageAsync(message.Chat.Id, $"Вы отправили - {message.Text}");

                    break;
                case MessageType.Photo:
                    break;
                case MessageType.Audio:
                    break;
                case MessageType.Video:
                    break;
                case MessageType.Voice:
                    break;
                case MessageType.Document:
                    break;
                case MessageType.Sticker:
                    break;
                case MessageType.Location:
                    break;
                case MessageType.Contact:
                    break;

                default:
                    break;
            }
        }

        #endregion 

        #endregion

    }
}
