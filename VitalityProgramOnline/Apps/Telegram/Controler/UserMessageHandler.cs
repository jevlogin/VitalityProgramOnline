using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using VitalityProgramOnline.Helper;
using VitalityProgramOnline.Models.Feedback;
using VitalityProgramOnline.Models.FoodDiary;
using VitalityProgramOnline.Models.User;
using VitalityProgramOnline.Models.User.Settings;


namespace VitalityProgramOnline.Apps.Telegram.Controler
{
    public class UserMessageHandler : MessageHandler
    {
        #region Fields

        private readonly DatabaseService _databaseService;
        private readonly Dictionary<long, ApplicationUser> _adminList;
        private Dictionary<long, ApplicationUser> _userList;
        private Dictionary<long, ProgressUsers> _progressUsersList = new Dictionary<long, ProgressUsers>();
        private readonly Dictionary<long, InlineKeyboardMarkup> _buttonContinueList = new Dictionary<long, InlineKeyboardMarkup>();
        private static Random _random = new Random();

        #endregion


        #region ClassLifeCycles

        public UserMessageHandler(TelegramBotClient botClient, DatabaseService databaseService, Dictionary<long, ApplicationUser> adminList, Dictionary<long, ApplicationUser> userList) : base(botClient)
        {
            _databaseService = databaseService;
            _adminList = adminList;
            _userList = userList;

            if (_userList.Count > 0)
            {
                ConnectedToDatabaseAndSynchronizeProgress().Wait();

                foreach (var progress in _progressUsersList.Values)
                {
                    progress.ProgressUpdated += ProgressUsersUpdatedAsync;
                    if (!progress.IsTheNextStepSheduledInTime || !progress.IsTheNextDaysUpdateIsCompleted)
                    {
                        progress.UpdateState = UpdateState.UpdateDate;
                    }
                }
            }
        }

        #endregion


        #region ProgressUsersUpdatedAsync

        private async void ProgressUsersUpdatedAsync(ProgressUsers progress)
        {
            if (_userList[progress.UserId] is { } user)
            {
                await Console.Out.WriteLineAsync($"Получены обновления прогресса пользователя - {user.FirstName} : {user.UserId}");
                await Console.Out.WriteLineAsync($"День: {progress.CurrentDay} - Шаг обновления: {progress.CurrentStep}");
            }
            else
            {
                await Console.Out.WriteLineAsync($"Ошибка синхронизации пользователей.");
                return;
            }

            var updateProgress = await _databaseService.UpdateUserProgressAsync(progress);

            if (updateProgress && progress.UpdateState != UpdateState.UpdateDate)   //1 exc
            {
                switch (progress.CurrentDay)
                {
                    case 1:
                        await UpdateAsyncDay1(progress);

                        break;
                    case 2:
                        await UpdateAsyncDay2(progress);

                        break;
                    default:
                        await Console.Out.WriteLineAsync($"Ошибка обновления прогресса пользователя.");
                        break;
                }
            }
        }

        #endregion


        #region UpdateAsyncDay2

        private async Task UpdateAsyncDay2(ProgressUsers progress)
        {
            switch (progress.CurrentStep)
            {
                case 1:
                    await _botClient.SendTextMessageAsync(progress.UserId, GetStringFormatDialogUser(DialogData.GOOD_MORNING, progress.UserId));
                    await Pause(1000, 2000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_1);
                    await Pause(1000, 2000);

                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    break;
                case 2:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_2, parseMode: ParseMode.Html);
                    await Pause(1500, 2000);

                    await CreateMenuSettingsBotAsync(progress.UserId, CancellationToken.None);
                    await Pause(1500, 2000);

                    await SetNextStepTimeAddMinuteAsync(progress, 1);

                    break;
                case 3:
                    await _botClient.SendTextMessageAsync(progress.UserId, GetStringFormatDialogUser(DialogData.DIALOG_DAY_2_STEP_3, progress.UserId));
                    await Pause(2000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    await Pause(2000);

                    break;
                case 4:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_4, parseMode: ParseMode.Html);
                    await Pause(2000, 3000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    await Pause(2000, 3000);

                    break;
                case 5:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_5, parseMode: ParseMode.Html);
                    await Pause(2000, 3000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);

                    break;
                case 6:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_6);
                    await SetNextStepTimeAddHoursAsync(progress, 10);

                    break;
                case 7:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_7_1, parseMode: ParseMode.Html);
                    await Pause(2000, 4000);

                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_7_2, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    await SetNextStepTimeAddMinuteAsync(progress, 2);

                    break;
                case 8:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.DIALOG_DAY_2_STEP_8, parseMode: ParseMode.Html);
                    await Pause(2000);

                    await SetNextDayDefaultOrUserSettings(progress);

                    break;
                default:
                    await _botClient.SendTextMessageAsync(progress.UserId, GetStringFormatDialogUser(DialogData.DIALOG_DEFAULT_CASE_1, progress.UserId));
                    await Pause(2000, 2500);
                    await _botClient.SendTextMessageAsync(progress.UserId, $"{_userList[progress.UserId].FirstName}, Напоминаю, ты также можешь задавать свои вопросы, прямо в чат бота.");
                    await Pause(1000, 2000);

                    foreach (var admin in _adminList.Keys)
                    {
                        await _botClient.SendTextMessageAsync(admin, $"Пользователь {_userList[progress.UserId].FirstName}, требует внимания и живого общения.");
                    }

                    await SetNextDayDefaultOrUserSettings(progress);

                    break;
            }
        }

        #endregion


        #region SetNextStepTimeAddHoursAsync

        private async Task SetNextStepTimeAddHoursAsync(ProgressUsers progress, int hour)
        {
            if (progress.IsTheNextStepSheduledInTime)
            {
                SetNextTimeStepAddHours(progress, hour);
            }
            else
            {
                await Console.Out.WriteLineAsync(DialogData.NOT_CHANGE_NEXT_STEP);
            }
        }

        private void SetNextTimeStepAddHours(ProgressUsers userProgres, int hour)
        {
            userProgres.DateTimeOfTheNextStep = DateTime.UtcNow.ToLocalTime().AddHours(hour);
        }

        #endregion


        #region CreateMenuSettingsBotAsync

        private async Task CreateMenuSettingsBotAsync(long chatId, CancellationToken none)
        {
            var webApp = new WebAppInfo();
            webApp.Url = @"https://jevlogin.github.io/VPO/BotConfig.html";
            var buttonChangeSettings = new KeyboardButton("/🛠️ Изменить настройки");
            buttonChangeSettings.WebApp = webApp;

            var buttonReadSettings = new KeyboardButton("/🧑🏻‍💻 Настройки посмотреть");

            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { buttonChangeSettings, buttonReadSettings },
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(chatId, DialogData.CHOOSE_ONE_OF_THE_OPTIONS, replyMarkup: replyKeyboard);
        }

        #endregion


        #region UpdateAsyncDay1

        private async Task UpdateAsyncDay1(ProgressUsers progress)
        {
            switch (progress.CurrentStep)
            {
                case 0:
                    await DialogZeroStepDayOne(progress.UserId);
                    await Pause(2000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    break;
                case 1:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.USER_VPO_OFFLINE_DAY_1_STEP_1, replyMarkup: new ReplyKeyboardRemove());
                    await Pause(2000);
                    //TODO - Тут добавить кейсы, результаты...
                    await SendingTheResultsOfTheProgramParticipants(progress.UserId);
                    await Pause();
                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    break;
                case 2:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.GEETING_TO_KNOW_HERBY, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                    var fotoResultsUrl = "https://raw.githubusercontent.com/jevlogin/VPO/main/images/Herby.jpg";
                    await SendPhotoAsync(progress.UserId, fotoResultsUrl);
                    //TODO - (Показываем визуализированный образ Герби, он машет рукой)
                    await Pause(3000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.GEETING_TO_KNOW_HERBY_TOGETHER, parseMode: ParseMode.Html);
                    await Pause(2000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    break;
                case 3:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.PREPARING_FOR_THE_TRIP, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                    await Pause(2000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.PREPARING_FOR_THE_TRIP_LOST_STEP, parseMode: ParseMode.Html);
                    await Pause(3000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);
                    break;
                case 4:
                    var msgJourneBegins = GetStringFormatDialogUser(DialogData.THE_JOURNEY_BEGINS_USERFIELDS_0, progress.UserId);

                    await _botClient.SendTextMessageAsync(progress.UserId, msgJourneBegins, parseMode: ParseMode.Html, replyMarkup: new ReplyKeyboardRemove());
                    await Pause(2000, 5000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.THE_JOURNEY_BEGINS_PHIRST_STEP, parseMode: ParseMode.Html);
                    await Pause(3000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.THE_JOURNEY_BEGINS_CONCLISION, parseMode: ParseMode.Html);
                    await Pause(2000);
                    await CreateMenuInlineKeyboardContinue(progress.UserId);

                    break;
                case 5:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.INTRODUCTORY_INFORMATION_ABOUT_THE_TRIP, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    var aquaGirlPercent80 = "https://raw.githubusercontent.com/jevlogin/VPO/main/images/aqua_girl_percent80.jpg";
                    await SendPhotoAsync(progress.UserId, aquaGirlPercent80);
                    await Pause(2000);

                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.WHAT_DOES_A_PERSON_CONSIST_OF, parseMode: ParseMode.Html);
                    await Pause(2000, 4000);

                    await _botClient.SendTextMessageAsync(progress.UserId, "Вы спросите какой еще такой дневник?\n\nДа вот же он -", parseMode: ParseMode.Html);
                    await Pause(2000, 3000);

                    var foodDiaryImage = "https://raw.githubusercontent.com/jevlogin/VPO/main/images/foodDiaryImage.jpg";
                    await SendPhotoAsync(progress.UserId, foodDiaryImage);
                    await Pause(2000);

                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.WHAT_IS_FOODDIARY_1, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.WHAT_IS_FOODDIARY_2, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    await _botClient.SendTextMessageAsync(progress.UserId, "Это пока все, что Вам надо знать, на данном этапе.", parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.INTRODUCTORY_INFORMATION_ABOUT_THE_TRIP_2, parseMode: ParseMode.Html);
                    await Pause(1200, 3000);

                    await SetNextStepTimeAddMinuteAsync(progress, 3);
                    await Pause(2000);

                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.BOT_ANSWER_GOODBUY, parseMode: ParseMode.Html);
                    break;
                case 6:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.WHAT_IS_FOODDIARY_3, parseMode: ParseMode.Html);
                    await Pause(3000, 5000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.SECRET_PASHAL_1, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.INTRODUCTORY_INFORMATION_ABOUT_THE_TRIP_2, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.VPO_PROGRAM_ZOOM, parseMode: ParseMode.Html);
                    await Pause(3000, 5000);
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.BOT_ANSWER_GOODBUY, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    await SetNextStepTimeAddMinuteAsync(progress, 3);

                    break;
                default:
                    await _botClient.SendTextMessageAsync(progress.UserId, DialogData.REMINDER_OF_DAY_1, parseMode: ParseMode.Html);
                    await Pause(1000, 2000);

                    await SetNextDayDefaultOrUserSettings(progress);

                    break;
            }
        }

        #endregion


        #region SetNextDayDefaultOrUserSettings

        private async Task SetNextDayDefaultOrUserSettings(ProgressUsers progres)
        {
            if (progres.IsTheNextDaysUpdateIsCompleted)
            {
                var userSettings = await _databaseService.ReadUserBotSettings(progres.UserId, CancellationToken.None) as UserBotSettings;
                if (userSettings is { } settings && settings.MorningTime is { } time && time.Hours is { } hour)
                {
                    SetNextDayHourInProgress(progres, hour);
                    await Pause(1000, 2000);
                }
                else
                {
                    SetNextDayHourInProgress(progres, 9);
                    await Pause(1000, 2000);
                }
            }
            else
            {
                await Console.Out.WriteLineAsync(DialogData.NOT_CHANGE_NEXT_STEP);
            }
        }

        private void SetNextDayHourInProgress(ProgressUsers userProgres, int hour)
        {
            var tomorrow = DateTime.Today.AddDays(1);
            var nextDay = new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, hour, 0, 0);

            userProgres.DateNextDay = nextDay;
        }

        #endregion


        #region SetNextStepTimeAddMinuteAsync

        private async Task SetNextStepTimeAddMinuteAsync(ProgressUsers progress, int time)
        {
            if (progress.IsTheNextStepSheduledInTime)
            {
                SetNextTimeStepAddMinutes(progress, time);
            }
            else
            {
                await Console.Out.WriteLineAsync(DialogData.NOT_CHANGE_NEXT_STEP);
            }
        }

        private void SetNextTimeStepAddMinutes(ProgressUsers userProgres, int minutes)
        {
            userProgres.DateTimeOfTheNextStep = DateTime.UtcNow.ToLocalTime().AddMinutes(minutes);
        }

        #endregion


        #region GetStringFormatDialogUser

        private string GetStringFormatDialogUser(string data, long userId)
        {
            return string.Format(data, _userList[userId].FirstName);
        }

        #endregion


        #region SendPhotoAsync

        private async Task SendPhotoAsync(long chatId, string urlPhoto)
        {
            var msgInstructionHowto = await LoadedInstruction(chatId);

            var resultsPhotoSend = await _botClient.SendPhotoAsync(chatId, InputFile.FromUri(urlPhoto));

            if (resultsPhotoSend != null)
            {
                await _botClient.DeleteMessageAsync(chatId, msgInstructionHowto.MessageId);
            }
        }

        #endregion


        #region SendingTheResultsOfTheProgramParticipants

        private async Task SendingTheResultsOfTheProgramParticipants(long userId)
        {
            await _botClient.SendTextMessageAsync(userId, DialogData.RESULTS_OF_OUR_PARTICIPANTS, replyMarkup: new ReplyKeyboardRemove());
            await Pause(1500);

            var msgInstructionHowto = await LoadedInstruction(userId);

            var fotoResultsUrl = "https://raw.githubusercontent.com/jevlogin/VPO/main/results/result_komolova.png";
            var resultsPhotoSend = await _botClient.SendPhotoAsync(userId, InputFile.FromUri(fotoResultsUrl));

            if (resultsPhotoSend != null)
            {
                await _botClient.DeleteMessageAsync(userId, msgInstructionHowto.MessageId);
            }
        }

        #endregion


        #region LoadedInstruction

        private async Task<Message> LoadedInstruction(long chatId)
        {
            return await _botClient.SendTextMessageAsync(chatId, "<i>Загружаю инструкцию...</i>", parseMode: ParseMode.Html);
        }

        #endregion


        #region CreateMenuInlineKeyboardContinue

        private async Task CreateMenuInlineKeyboardContinue(long userId)
        {
            var answer = DialogData.USER_CONTINUER_RESPONSE_BUTTON[_random.Next(0, DialogData.USER_CONTINUER_RESPONSE_BUTTON.Length)];
            var button = InlineKeyboardButton.WithCallbackData(answer, callbackData: "/user_continue");
            var replyMarkup = new InlineKeyboardMarkup(button);
            var positivePhrase = DialogData.USER_MOTIVATIONAL_PHRASES[_random.Next(0, DialogData.USER_MOTIVATIONAL_PHRASES.Length)];
            await _botClient.SendTextMessageAsync(userId, positivePhrase, parseMode: ParseMode.Html, replyMarkup: replyMarkup);
            _buttonContinueList[userId] = replyMarkup;
        }

        #endregion


        #region DialogZeroStepDayOne

        private async Task DialogZeroStepDayOne(long userId)
        {
            await _botClient.SendTextMessageAsync(userId, DialogData.HERBY_WELCOME_TO_VPO, parseMode: ParseMode.Html);
            await Pause(1200, 2000);
            await _botClient.SendTextMessageAsync(userId, DialogData.HERBY_PROGRAM_PRESENTATION_RIDDLES, parseMode: ParseMode.Html);
        }

        #endregion


        #region Pause

        public async Task Pause(int? valueMax = null, int? valueMin = null)
        {
            int minV = 500;
            int maxV = 5000;
            int result = minV;

            if (valueMin.HasValue && valueMax.HasValue)
            {
                if (valueMin > valueMax)
                {
                    var temp = valueMin;
                    valueMin = valueMax;
                    valueMax = temp;
                }
                result = _random.Next(valueMin.Value, valueMax.Value);
            }
            else if (valueMin.HasValue)
            {
                result = _random.Next(valueMin.Value, maxV);
            }
            else if (valueMax.HasValue)
            {
                result = _random.Next(minV, valueMax.Value);
            }
            else
            {
                result = _random.Next(minV, maxV);
            }

            await Task.Delay(result);
        }

        #endregion


        #region ConnectedToDatabaseAndSynchronizeProgress

        private async Task ConnectedToDatabaseAndSynchronizeProgress()
        {
            _progressUsersList = await _databaseService.LoadProgressUsersAsync();
            await Console.Out.WriteLineAsync($"прогресс пользователей, был УСПЕШНО загружен.");
        }

        #endregion


        #region IMessageHandler


        #region HandlePollingErrorAsync

        public override async Task HandlePollingErrorAsync(Exception exception, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync($"An error occurred during handling user message: {exception}");

            if (exception is ApiRequestException apiException)
            {
                await Console.Out.WriteLineAsync($"API error occurred: {apiException.ErrorCode} - {apiException.Message}");
            }
            else
            {
                await Console.Out.WriteLineAsync("An unknown error occurred.");
            }
            await Task.CompletedTask;
        }

        #endregion


        #region HandleUpdateAsync

        public override async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:

                    if (update.Message is not { } message)
                        return;

                    switch (message.Type)
                    {
                        case MessageType.Unknown:
                            await Console.Out.WriteLineAsync($"Бог знает что пишет этот пользователь - {message.Chat.Id}\n{message}");
                            break;
                        case MessageType.Text:
                            if (message.Text is not { } text)
                                return;
                            if (text.StartsWith('/'))
                            {
                                await HandleCommandMessageAsync(message, cancellationToken);
                            }
                            else
                            {
                                await HandleTextMessageAsync(message, cancellationToken);
                            }
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
                        case MessageType.WebAppData:
                            if (message.WebAppData is not { } webAppData)
                                return;

                            await _botClient.SendTextMessageAsync(message.Chat.Id, $"Данные получены! ❤ 👌 ✔", replyMarkup: new ReplyKeyboardRemove());
                            await Pause(500);

                            try
                            {
                                await ParseWebAppData(message, webAppData, cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                await Console.Out.WriteLineAsync($"Не удалось обработать данные.{ex.Message}");
                            }

                            break;
                        default:
                            await Console.Out.WriteLineAsync($"Бог знает что пишет этот пользователь - {message.Chat.Id}\n");
                            break;
                    }
                    break;
                case UpdateType.CallbackQuery:
                    if (update.CallbackQuery is { } callbackQuery)
                    {
                        await HandleCallBackQueryAsync(callbackQuery, cancellationToken);
                    }
                    break;
                case UpdateType.EditedMessage:
                    break;

                default:

                    break;
            }
        }

        #endregion


        #region IsAdmin

        public override bool IsAdmin(long userId)
        {
            if (!_adminList.ContainsKey(userId))
            {
                return true;
            }
            return false;
        }

        #endregion


        #endregion


        #region HandleCommandMessageAsync

        private async Task HandleCommandMessageAsync(Message message, CancellationToken cancellationToken)
        {
            if (message.Text is { } text)
            {
                await Console.Out.WriteLineAsync($"{text}");
                var commands = text.ToLower().Split(' ');
                var command1 = commands[0];

                switch (command1)
                {
                    case "/start":
                        if (_userList.TryGetValue(message.Chat.Id, out var user))
                        {
                            if (user.UserId is { } userId)
                                _progressUsersList[userId].UpdateState = UpdateState.FullUpdate;

                            await Pause(5000);
                            await _botClient.SendTextMessageAsync(message.Chat.Id, DialogData.START_HELP_MESSAGE);
                        }
                        else
                        {
                            await _botClient.SendTextMessageAsync(message.Chat.Id,
                                DialogData.HERBY_IINTRODUCE_YOURSELF, parseMode: ParseMode.Html);
                            await Pause(1500);
                            await _botClient.SendTextMessageAsync(message.Chat.Id,
                                DialogData.WHAT_IS_YOUR_NAME, parseMode: ParseMode.Html);
                            await Pause(1000);
                            await CreateMenuKeyboardAuthUser(message.Chat.Id, cancellationToken);
                            await Pause(2000, 3000);

                            var predstavitsya_1 = "https://raw.githubusercontent.com/jevlogin/VPO/main/images/Intro1.jpg";
                            var predstavitsya_2 = "https://raw.githubusercontent.com/jevlogin/VPO/main/images/Intro2.jpg";

                            await SendPhotoAsync(message.Chat.Id, predstavitsya_1);
                            await Pause(1000, 2000);
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "Еслии ты ее не видишь, жми на 4 квадратика!!! Вот наглядно куда нажать надо.");
                            await SendPhotoAsync(message.Chat.Id, predstavitsya_2);

                        }
                        break;
                    case "/menu":
                        await CreateMenuKeyboard(message.Chat.Id, cancellationToken);
                        break;
                    case "/help":
                        await _botClient.SendTextMessageAsync(
                            message.Chat.Id,
                            DialogData.HELP_MENU_BUTTON, parseMode: ParseMode.Html,
                            cancellationToken: cancellationToken, replyMarkup: new ReplyKeyboardRemove());
                        break;
                    default:
                        if (commands.Length > 1) { break; }
                        await _botClient.SendTextMessageAsync(message.Chat.Id,
                            "Извините, я не узнаю эту команду.",
                            cancellationToken: cancellationToken);
                        break;
                }

                if (commands.Length > 1)
                {
                    var command2 = commands[1];
                    switch (command2)
                    {
                        case "feedback":
                            if (commands.Length <= 2)
                            {
                                await CreateMenuFeedback(message.Chat.Id, cancellationToken);
                            }
                            else if (commands[2] is { } feedbackHelp)
                            {
                                switch (feedbackHelp)
                                {
                                    case "посмотреть":
                                        await _botClient.SendTextMessageAsync(message.Chat.Id, "Скоро Вы сможете посмотреть свои ответы");
                                        break;
                                }
                            }
                            else
                            {
                                await SendMessageCommingSoonAsync(message.Chat.Id, cancellationToken);
                            }

                            break;
                        case "дневник":
                            if (commands[2] is { } foodDiarryCommand)
                            {
                                switch (foodDiarryCommand)
                                {
                                    case "питания":
                                        await CreateMenuFoodDiaryAsync(message.Chat.Id, cancellationToken);

                                        break;
                                    default:
                                        Console.WriteLine("Кто-то, что-то попутал.");
                                        break;
                                }
                            }
                            break;
                        case "прочитать":
                            if (commands[2] is { } foodDiarryFilling)
                            {
                                switch (foodDiarryFilling)
                                {
                                    case "дневник":
                                        await Console.Out.WriteLineAsync($"мы нажали прочитать дневник питания");
                                        var foodDiaryList = await _databaseService.ReadTheFoodDiaryForTheCurrentDay(message.Chat.Id, cancellationToken);
                                        await Pause(1000, 1500);
                                        await _botClient.SendTextMessageAsync(message.Chat.Id, DialogData.BORDER_FRUIT, cancellationToken: cancellationToken);
                                        foreach (var foodDiary in foodDiaryList)
                                        {
                                            await _botClient.SendTextMessageAsync(message.Chat.Id, foodDiary.ToString(), cancellationToken: cancellationToken);
                                        }
                                        await Pause(1000, 1500);
                                        await _botClient.SendTextMessageAsync(message.Chat.Id, DialogData.BORDER_FRUIT, cancellationToken: cancellationToken);

                                        break;
                                    default:
                                        Console.WriteLine("Кто-то, что-то попутал.");
                                        break;
                                }
                            }
                            break;
                        case "настройки":
                            if (commands[2] is { } settingsUserCommand)
                            {
                                switch (settingsUserCommand)
                                {
                                    case "пользователя":
                                        Console.WriteLine("Даем возможность пользователю, изменить настройки, перепрограммировать");
                                        await CreateMenuSettingsBotAsync(message.Chat.Id, cancellationToken);

                                        break;
                                    case "посмотреть":
                                        await Console.Out.WriteLineAsync($"Кто-то хочет узнать настройки, а значит их надо где-то хранить.");
                                        var settingsUser = await _databaseService.ReadUserBotSettings(message.Chat.Id, cancellationToken);

                                        await WriteUserBotSettingsAsync(message, settingsUser);

                                        break;
                                    default:
                                        Console.WriteLine("Кто-то, что-то попутал.");
                                        break;
                                }
                            }
                            break;
                        case "офлайн":
                            if (commands[2] is { } command3)
                            {
                                switch (command3)
                                {
                                    case "консультация":
                                        if (UserAutorization(message.Chat.Id))
                                        {
                                            var answer = await _botClient.SendTextMessageAsync(message.Chat.Id,
                                                $"{_userList[message.Chat.Id].FirstName}\nНа чем мы тут остановились?",
                                                replyMarkup: new ReplyKeyboardRemove());
                                            await Pause(1000);

                                            await _botClient.SendTextMessageAsync(message.Chat.Id, "🤖 " +
                                                DialogData.BOT_ANSWER_SMILE_PRANK_ARRAY[_random.Next(0, DialogData.BOT_ANSWER_SMILE_PRANK_ARRAY.Length)],
                                                parseMode: ParseMode.Html);
                                            await Pause(1000);

                                            _progressUsersList[message.Chat.Id].UpdateState = UpdateState.FullUpdate;
                                        }
                                        else
                                        {
                                            await _botClient.SendTextMessageAsync(message.Chat.Id, DialogData.CONSULTATION_OFFLINE_WELCOME);
                                            await CreateMenuKeyboardAuthUser(message.Chat.Id, cancellationToken);
                                        }
                                        break;
                                    default:
                                        await _botClient.SendTextMessageAsync(message.Chat.Id, "Извините, я не узнаю эту команду.");
                                        break;
                                }
                                break;
                            }
                            break;
                        case "/exit" or "exit" or "/выход" or "выход":
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "До свидания!", replyMarkup: new ReplyKeyboardRemove());
                            break;
                        default:
                            await _botClient.SendTextMessageAsync(message.Chat.Id, "Извините, я не узнаю эту команду.");
                            break;
                    }
                }
            }
        }

        #endregion


        #region UserAutorization

        private bool UserAutorization(long id)
        {
            return _userList.ContainsKey(id);
        }

        #endregion


        #region CreateMenuFoodDiaryAsync

        private async Task CreateMenuFoodDiaryAsync(long chatId, CancellationToken cancellationToken)
        {
            var webApp = new WebAppInfo();
            webApp.Url = @"https://jevlogin.github.io/VPO/FoodDiary.html";
            var buttonFillingFoodDiary = new KeyboardButton("/📖 Заполнить дневник");
            buttonFillingFoodDiary.WebApp = webApp;

            var buttonReadFoodDiary = new KeyboardButton("/📖 Прочитать дневник");

            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { buttonFillingFoodDiary, buttonReadFoodDiary },
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(chatId, DialogData.CHOOSE_ONE_OF_THE_OPTIONS, replyMarkup: replyKeyboard);
        }

        #endregion


        #region SendMessageCommingSoonAsync

        private async Task SendMessageCommingSoonAsync(long id, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync(id, DialogData.THE_TECHNOLOGY_IS_UNDER_DEVELOPMENT, replyMarkup: new ReplyKeyboardRemove());
        }

        #endregion


        #region CreateMenuFeedback

        private async Task CreateMenuFeedback(long chatId, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync(chatId, "Мы активно развиваем этот проект. И нам требуется обратная связь.\nПожалуйста помоги сделать проект ближе к людям! ❤");

            var webApp = new WebAppInfo();
            webApp.Url = @"https://jevlogin.github.io/VPO/Feedback.html";
            var buttonFeedback = new KeyboardButton("/🛠️ Помочь в развитии проекта");
            buttonFeedback.WebApp = webApp;

            var buttonFeedbackMyAnswer = new KeyboardButton("/🧑🏻‍💻 Feedback посмотреть мои ответы");

            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { buttonFeedback, buttonFeedbackMyAnswer },
            })
            {
                ResizeKeyboard = true,
            };

            await _botClient.SendTextMessageAsync(chatId, DialogData.CHOOSE_ONE_OF_THE_OPTIONS, replyMarkup: replyKeyboard);
        }

        #endregion


        #region CreateMenuKeyboard

        private async Task CreateMenuKeyboard(long chatId, CancellationToken cancellationToken)
        {
            var webAppInfo = new WebAppInfo();
            webAppInfo.Url = @"https://jevlogin.github.io/VPO/RequestForConsultation.html";
            var consultationOnlineButton = new KeyboardButton("/📞 Записаться на консультацию");
            consultationOnlineButton.WebApp = webAppInfo;

            var consultationOfflineButton = new KeyboardButton("/🧬 Офлайн консультация");
            var feedbackButton = new KeyboardButton("/🐾 Feedback");
            var exitButton = new KeyboardButton("/🏠 Выход");

            var foodDiarryButton = new KeyboardButton("/📖 Дневник питания");
            var settingsButton = new KeyboardButton("/☸ Настройки пользователя");

            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { consultationOnlineButton, consultationOfflineButton  },
                new[] { foodDiarryButton, settingsButton },
                new[] { feedbackButton, exitButton },
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(
                chatId: chatId,
                text: DialogData.CHOOSE_ONE_OF_THE_OPTIONS,
                replyMarkup: replyKeyboard);
        }

        #endregion


        #region HandleTextMessageAsync

        private async Task HandleTextMessageAsync(Message message, CancellationToken cancellationToken)
        {
            await Console.Out.WriteLineAsync($"{message.Text}");

            foreach (var adminId in _adminList.Keys)
            {
                await _botClient.ForwardMessageAsync(
                        adminId,
                        message.Chat.Id,
                        message.MessageId,
                        cancellationToken: cancellationToken);
            }

            await _botClient.SendTextMessageAsync(message.Chat.Id, DialogData.YOUR_MESSAGE_HAS_BEEN_RECEIVED, cancellationToken: cancellationToken);
        }

        #endregion


        #region ParseWebAppData

        private async Task ParseWebAppData(Message message, WebAppData webAppData, CancellationToken cancellationToken)
        {
            var parseArray = JArray.Parse(webAppData.Data);

            JObject messageDataInfoType = (JObject)parseArray[0];

            MessageDataInfoType messageInfo;

            try
            {
                messageInfo = messageDataInfoType.ToObject<MessageDataInfoType>();
            }
            catch (Exception exception)
            {
                await Console.Out.WriteLineAsync($"Возникло исключение:\n\n{exception}");
                return;
            }

            if (messageInfo is { } msgInfo && msgInfo.CallBackMethod is { } callBackType)
            {
                switch (callBackType)
                {
                    case CallBackMethod.Feedback:
                        await _botClient.SendTextMessageAsync(message.From.Id, "Мы получили ваши данные. Обрабатываем");
                        await Console.Out.WriteLineAsync($"Мы получили обратнуюсвязь от пользователя.");

                        JObject feedbackResponseObject = (JObject)parseArray[1];

                        FeedbackResponse feedbackResponse;
                        try
                        {
                            feedbackResponse = feedbackResponseObject.ToObject<FeedbackResponse>();
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync($"Возникло исключение:\n\n{ex}");
                            return;
                        }

                        var userExist = await _databaseService.LoadUserAnyAsync(message.From.Id);
                        if (!userExist)
                        {
                            var username = DialogData.ANONYMOUS_USERNAME[_random.Next(0, DialogData.ANONYMOUS_USERNAME.Length)];
                            var tempUser = new ApplicationUser
                            {
                                UserId = message.From.Id,
                                IsAdmin = false,
                                FirstName = message.From.FirstName ?? username
                            };
                            await AddedNewUserToLocalUserList(tempUser);
                            await _databaseService.AddOrUpdateUserAsync(tempUser);

                            await AddedNewUserProgressInLocalListAndSubscribeUpdate(day: 1, step: 0, user: tempUser);
                            await _databaseService.UpdateUserProgressAsync(_progressUsersList[message.From.Id]);

                            feedbackResponse.Id = tempUser.Id;
                        }
                        else
                        {
                            feedbackResponse.Id = _userList[message.From.Id].Id;
                        }

                        await _databaseService.AddOrUpdateFeedbackResponseAsync(feedbackResponse);

                        break;
                    case CallBackMethod.BotConfig:
                        await Console.Out.WriteLineAsync($"Мы получили настройки пользователя.");
                        JObject botSettingsObject = (JObject)parseArray[1];
                        UserBotSettings userBotSettings;
                        try
                        {
                            userBotSettings = botSettingsObject.ToObject<UserBotSettings>();
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync($"Возникло исключение:\n\n{ex}");
                            return;
                        }
                        userBotSettings!.UserId = message.From!.Id;

                        if (!_userList.Keys.Contains(message.From.Id))
                        {
                            await _botClient.SendTextMessageAsync(message.From.Id, DialogData.AVAILABLE_ONLY_TO_AUTHORIZED_USERS);
                            await Pause(1000, 2000);
                            await _botClient.SendTextMessageAsync(message.From.Id, $"Вот так могли бы выглядеть Ваши настройки:\n\n");
                            await Pause(1000, 2000);
                            await WriteUserBotSettingsAsync(message, userBotSettings);
                            await _botClient.SendTextMessageAsync(message.From.Id, DialogData.SUGGEST_QUICK_REGISTRATION);
                            await Pause(1000, 2000);
                            await CreateMenuKeyboardAuthUser(message.From.Id, cancellationToken);
                        }
                        else
                        {
                            userBotSettings.Id = _userList[message.From.Id].Id;

                            await _databaseService.AddOrUpdateBotSettingsAsync(userBotSettings);
                            await Pause(1000, 2000);
                            //TODO - изменить настройки пользователя..
                            if (userBotSettings.MorningTime is { } time)
                            {
                                _progressUsersList[message.From.Id].DateNextDay.ChangeDate(time);
                            }

                            await _botClient.SendTextMessageAsync(message.From.Id, $"Вот Ваша запись:\n\n");
                            await Pause(1000, 2000);
                            await WriteUserBotSettingsAsync(message, userBotSettings);
                            await _botClient.SendTextMessageAsync(message.From.Id, $"Вы можете быстро посмотреть свои настройки в соответствующем пункте меню.\n");
                        }

                        break;
                    case CallBackMethod.FoodDiaryFilling:

                        JObject foodDiaryForm = (JObject)parseArray[1];
                        var foodDiary = foodDiaryForm.ToObject<FoodDiaryEntry>();

                        if (foodDiary != null)
                        {
                            foodDiary.UserId = message.From.Id;

                            Console.WriteLine(foodDiary.ToString());

                            if (!_userList.Keys.Contains(message.From.Id))
                            {
                                await _botClient.SendTextMessageAsync(message.From.Id, DialogData.AVAILABLE_ONLY_TO_AUTHORIZED_USERS);
                                await Pause(1000, 2000);
                                await _botClient.SendTextMessageAsync(message.From.Id, $"Вот так могла бы выглядеть Ваша запись:\n\n");
                                await Pause(1000, 2000);
                                await _botClient.SendTextMessageAsync(message.From.Id, foodDiary.ToString());
                                await Pause(4000, 6000);
                                await _botClient.SendTextMessageAsync(message.From.Id, DialogData.SUGGEST_QUICK_REGISTRATION);
                                await CreateMenuKeyboardAuthUser(message.From.Id, cancellationToken);
                            }
                            else
                            {
                                foodDiary.UserIdForeignKey = _userList[message.From.Id].Id;

                                await _databaseService.AddOrUpdateFoodDiaryAsync(foodDiary);

                                await Pause(1000, 2000);
                                await _botClient.SendTextMessageAsync(message.From.Id, $"Вот Ваша запись:\n\n");
                                await Pause(1000, 2000);
                                await _botClient.SendTextMessageAsync(message.From.Id, foodDiary.ToString());
                                await Pause(1000, 2000);
                                await _botClient.SendTextMessageAsync(message.From.Id, $"Вы можете посмотреть свой дневник в соответствующем пункте меню.\n");
                            }
                        }

                        break;
                    case CallBackMethod.UserIntroduction:
                        JObject vpoForm = (JObject)parseArray[1];
                        var user = vpoForm.ToObject<ApplicationUser>();

                        if (user != null)
                        {
                            user.UserId = message.From.Id;

                            if (!_userList.Keys.Contains(user.UserId!.Value))
                            {
                                await AddedNewUserToLocalUserList(user);

                                var greeting = DialogData.GREETING_TEMPLATES_STRING_FORMAT[_random.Next(0, DialogData.GREETING_TEMPLATES_STRING_FORMAT.Length)];
                                string greetingMessage = GetStringFormatDialogUser(greeting, user.UserId!.Value);
                                var msgToUserIntro = await _botClient.SendTextMessageAsync(message.Chat.Id, greetingMessage, parseMode: ParseMode.Html);
                                await Pause(2000);

                                if (msgToUserIntro != null)
                                {
                                    await _databaseService.AddOrUpdateUserAsync(user);
                                    //может быть стоит тут теперь извлекать пользователя из базы, а потом создавать прогресс

                                    await AddedNewUserProgressInLocalListAndSubscribeUpdate(day: 1, step: 0, user: user);
                                    await _databaseService.UpdateUserProgressAsync(_progressUsersList[user.UserId!.Value]);
                                }
                                await Pause(700, 1500);

                                _progressUsersList[user.UserId!.Value].UpdateState = UpdateState.FullUpdate;
                            }
                            else
                            {
                                await Console.Out.WriteLineAsync("Ошибка. Пользователь должен был быть подгружен при старте программы.");
                            }
                        }

                        break;
                    case CallBackMethod.UserRequestForConsultation:

                        Console.WriteLine($"Получены данные: \"Заявка на консультацию\"");
                        JObject requestForConsultation = (JObject)parseArray[1];
                        var userRequest = requestForConsultation.ToObject<ApplicationUser>();

                        if (userRequest != null)
                        {
                            userRequest.UserId = message.From!.Id;

                            

                            if (!_userList.ContainsKey(userRequest.UserId.Value))
                            {
                                await AddedNewUserToLocalUserList(userRequest);
                                var msgToUserIntro = await _botClient.SendTextMessageAsync(userRequest.UserId, $"Отлично! Рад знакомству, {userRequest.FirstName}");

                                if (msgToUserIntro != null)
                                {
                                    /* Эксперимент создаю пароль */
                                    await _databaseService.AddOrUpdateUserAsync(userRequest);
                                    await _botClient.SendTextMessageAsync(userRequest.UserId, "Ваш временный пароль для входа в сервис - 'tempPWD@1'");

                                    await AddedNewUserProgressInLocalListAndSubscribeUpdate(day: 1, step: 0, user: userRequest);
                                    await _databaseService.UpdateUserProgressAsync(_progressUsersList[userRequest.UserId.Value]);

                                }
                                await SendMessageToAdminsForOnlineConsultation(userRequest, cancellationToken);
                            }
                            else
                            {
                                var newlocalUser = _userList[userRequest.UserId.Value].UpdateUser(userRequest);
                                //await AddedNewUserToLocalUserList(userRequest);
                                await _databaseService.AddOrUpdateUserAsync(newlocalUser);

                                await _botClient.SendTextMessageAsync(userRequest.UserId,
                                    $"{userRequest.FirstName}, я тебя помню ✌😊 и записал тебя на консультацию.");
                                await SendMessageToAdminsForOnlineConsultation(userRequest, cancellationToken);
                            }
                        }
                        break;
                    default:
                        await Console.Out.WriteLineAsync($"Ошибка. callBackType в message.WebAppData не верный");
                        break;
                }
            }
        }

        #endregion


        #region RequestConsultation

        private async Task SendMessageToAdminsForOnlineConsultation(ApplicationUser userFormRequest, CancellationToken cancellationToken)
        {
            await _botClient.SendTextMessageAsync(_adminList.FirstOrDefault().Key,
              $"Пользователь:\n\n" +
              $"Имя - {userFormRequest.FirstName}\n" +
              $"Фамилия - {userFormRequest.LastName}\n" +
              $"Телефон - {userFormRequest.PhoneNumber}\n" +
              (string.IsNullOrEmpty(userFormRequest.Email) ?
                  "" : $"Email - {userFormRequest.Email}\n\n") +
              $"\nЗаписался на <b>ОНЛАЙН КОНСУЛЬТАЦИЮ!</b>",
              parseMode: ParseMode.Html, cancellationToken: cancellationToken);
        }

        #endregion


        #region CreateMenuKeyboardAuthUser

        private async Task CreateMenuKeyboardAuthUser(long chatId, CancellationToken cancellationToken)
        {
            var webAppInfo = new WebAppInfo();
            webAppInfo.Url = @"https://jevlogin.github.io/VPO/IntroVPOBot.html";

            var button = new KeyboardButton("👽 Представиться 🤝");
            button.WebApp = webAppInfo;

            var replyKeyboard = new ReplyKeyboardMarkup(new[]
            {
                button
            })
            {
                ResizeKeyboard = true
            };

            await _botClient.SendTextMessageAsync(chatId, DialogData.BUTTON_AUTH_DOWN, replyMarkup: replyKeyboard);
        }

        #endregion


        #region WriteUserBotSettingsAsync

        private async Task WriteUserBotSettingsAsync(Message message, EmptyBotSettings userBotSettings)
        {
            await _botClient.SendTextMessageAsync(message.From.Id, userBotSettings.ToString(), parseMode: ParseMode.Html);
            await Pause(1500, 3000);
        }

        #endregion


        #region AddedNewUserProgressInLocalListAndSubscribeUpdate

        private async Task AddedNewUserProgressInLocalListAndSubscribeUpdate(int day, int step, ApplicationUser user)
        {
            await Console.Out.WriteLineAsync($"Создаю прогресс нового пользователя {user.FirstName} - День {day}, Шаг {step}");

            var currentTime = DateTime.UtcNow.ToLocalTime();

            var currentProgressOfflineUser = new ProgressUsers(user.UserId!.Value, user.Id, day, step, currentTime, currentTime,
                                                CheckingTheNextStepInTime(currentTime), UpdateState.None);

            await Console.Out.WriteLineAsync($"Добавляем прогресс пользователя в локальную базу данных.");
            _progressUsersList[user.UserId!.Value] = currentProgressOfflineUser;
            _progressUsersList[user.UserId!.Value].ProgressUpdated += ProgressUsersUpdatedAsync;
        }

        #endregion


        #region CheckingTheNextStepInTime

        private bool CheckingTheNextStepInTime(DateTime nextDateEventStep)
        {
            if (DateTime.UtcNow.ToLocalTime() < nextDateEventStep)
                return false;
            return true;
        }

        #endregion


        #region AddedNewUserToLocalUserList

        private async Task AddedNewUserToLocalUserList(ApplicationUser user)
        {
            if (!_userList.ContainsKey(user.UserId!.Value))
            {
                _userList[user.UserId.Value] = user;
                await Console.Out.WriteLineAsync($"Пользователь {user.FirstName}, был добавлен в локальный список пользователей.");
            }
            else
            {
                _userList[user.UserId.Value].UpdateUser(user);
                await Console.Out.WriteLineAsync($"Пользователь с id {user.UserId} уже существует в локальном списке пользователей. Данные обновлены.");
            }
        }

        #endregion


        #region HandleCallBackQueryAsync

        private async Task HandleCallBackQueryAsync(CallbackQuery callbackQuery, CancellationToken cancellationToken)
        {
            if (callbackQuery.Message is not { } message)
            {
                return;
            }
            var chatId = message.Chat.Id;
            var data = callbackQuery.Data;

            switch (data)
            {
                case "/user_continue":
                    if (_progressUsersList.Count > 0)
                    {
                        var msgAnswerCongrulatory = DialogData.USER_CONGRATILATORY_RESPONSES_ANSWER[_random.Next(0, DialogData.USER_CONGRATILATORY_RESPONSES_ANSWER.Length)];

                        _buttonContinueList[chatId] = new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData("👌 Спасибо"));
                        await _botClient.EditMessageReplyMarkupAsync(chatId, messageId: callbackQuery.Message.MessageId, replyMarkup: _buttonContinueList[chatId]);
                        await Pause(1500, 2000);
                        await _botClient.EditMessageReplyMarkupAsync(chatId, messageId: callbackQuery.Message.MessageId, replyMarkup: null);

                        await _botClient.SendTextMessageAsync(chatId, msgAnswerCongrulatory, parseMode: ParseMode.Html);
                        await Pause(1000, 2000);

                        _progressUsersList[chatId].CurrentStep++;
                    }
                    else
                    {
                        await Console.Out.WriteLineAsync($"Пользователей нет.");
                    }
                    break;
            }
        }

        #endregion
    }
}
