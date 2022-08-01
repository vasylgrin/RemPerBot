using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class BotController : BotControllerBase
    {
        #region Variable

        private int button = 0;
        private string inputText = "";

        #endregion
       
        /// <summary>
        /// Receives text and forwards it.
        /// </summary>
        /// <param name="messageText">Data.</param>
        /// <param name="chatID">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async void CheckAnswerAsync(string messageText, long chatID, CancellationToken cancellationToken)
        {
            vBotControllerBase(chatID, cancellationToken);

        #region validations answer

        start:
            if (messageText == "/start")
            {
                await PrintMessage("Привіт", chatID);
                await PrintMessage("Я твій помічник у двох запиатаннях:", chatID);
                await PrintMessage("Нагадування та період овуляції", chatID);
                await PrintKeyboard("Обирай дію:", chatID, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
            }
            else if (messageText == ReminderCommands.Нагадування.ToString())
            {
                await PrintKeyboard("Обирай:", chatID, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                button = 1;
            }
            else if (messageText == ReminderCommands.Додати.ToString())
            {
                inputText = "addReminder";
                button = 2;
            }
            else if (messageText == ReminderCommands.Переглянути.ToString())
            {
                new ReminderController().DisplayReminder(chatID, cancellationToken);
            }
            else if (messageText == PeriodCommands.Період.ToString())
            {
                await PrintKeyboard("Обирай:", chatID, SetupKeyboard(PeriodCommands.Перiод.ToString(), NotifyTheUserCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                button = 3;
            }
            else if (messageText == PeriodCommands.Перiод.ToString())
            {
                await PrintKeyboard("Обирай:", chatID, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                inputText = "";
                button = 4;
            }
            else if (messageText == PeriodCommands.Створити.ToString())
            {
                inputText = "addPeriod";
            }
            else if (messageText == PeriodCommands.Проглянути.ToString())
            {
                inputText = "";
                new PeriodController().DisplayPeriod(chatID, cancellationToken);
            }
            else if (messageText == PeriodCommands.Розпочались.ToString())
            {
                new PeriodController().UpdatePeriodAsync(chatID);
            }
            else if (messageText == NotifyTheUserCommands.Користувачі.ToString())
            {
                await PrintMessage("Ця функція створена для того щоб повідомити твого друга/подругу, хлопця і т.д. у яких є телеграм, про те що утебе почались \"ці дні\",і якщо у тебе є їхній id, то тобі залишається лише ввести його та ім'я людини(будь-яке яке захочеш) і надіслати.", chatID);
                await PrintMessage("Дізнатись свій Chat Id можна на початку додатку при натисканні /start, або у цій вкладці натиснувши myId.", chatID);
                await PrintKeyboard("Обирай:", chatID, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                button = 4;
            }
            else if (messageText == NotifyTheUserCommands.Добавити.ToString())
            {
                inputText = "addPersonForPeriod";
            }
            else if (messageText == NotifyTheUserCommands.Продивитись.ToString())
            {
                new NotifyTheUserController().DispalyNotifyTheUser(chatID, cancellationToken);
            }
            else if (messageText == GeneralCommands.Назад.ToString())
            {
                inputText = "";
                switch (button)
                {
                    case 1:
                        await PrintKeyboard("Обирай дію:", chatID, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                        return;
                    case 2:
                        messageText = ReminderCommands.Нагадування.ToString();
                        goto start;
                    case 3:
                        await PrintKeyboard("Обирай дію:", chatID, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                        return;
                    case 4:
                        messageText = PeriodCommands.Період.ToString();
                        goto start;
                    case 5:
                        messageText = PeriodCommands.Перiод.ToString();
                        goto start;
                    case 6:
                        await PrintKeyboard("Обирай:", chatID, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                        button = 4;
                        return;
                    default:
                        await PrintMessage("Щось не так.\nНатискай /start.", chatID);
                        break;
                }
            }
            else if (messageText == GeneralCommands.myId.ToString())
            {
                await PrintMessage($"Id твого чату:", chatID);
                await PrintMessage($"{chatID}", chatID);
            }
            else
            {
                inputText += messageText;
            }

            #endregion 

            CheckCommandsAsync(inputText, chatID, cancellationToken);
        }

        /// <summary>
        /// Receives data and forwards it further for processing.
        /// </summary>
        /// <param name="text">Data.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="token">Token.</param>
        private void CheckCommandsAsync(string text, long chatId, CancellationToken token)
        {
            if (text.Contains("addReminder"))
            {
                new ReminderController().AddReminder(chatId, text, out inputText, token);
            }
            else if (text.Contains("addPeriod"))
            {
                new PeriodController().AddPeriod(chatId, text, out inputText, out button, token);
            }
            else if (text.Contains("addPersonForPeriod"))
            {
                new NotifyTheUserController().AddNotifyTheUser(chatId, text, out inputText, out button, token);
            }
        }

        /// <summary>
        /// Receives the callback and forwards it.
        /// </summary>
        /// <param name="callbackQuery">Callback</param>
        public void CallbackQueryAsync(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteReminder.ToString()))
            {
                new ReminderController().DeleteReminder(callbackQuery);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deletePeriod.ToString()))
            {
                new PeriodController().DeletePeriod(callbackQuery);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteNotifyTheUser.ToString()))
            {
                new NotifyTheUserController().DeleteNotifyTheUser(callbackQuery);
            }
        }
    }
}

