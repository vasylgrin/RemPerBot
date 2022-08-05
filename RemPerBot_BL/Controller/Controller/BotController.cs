using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class BotController : BotControllerBase
    {
        #region Variable
        
        Dictionary<long, byte> NavigationDictionary = new();
        Dictionary<long, string> InputTextDictionary = new();
        
        ReminderController reminderController = new();
        PeriodController periodController = new();
        NotifyTheUserController notifyTheUserController = new();
        
        const string addReminder = "addReminder";
        const string addPeriod = "addPeriod";
        const string addNotifyTheUser = "addNotifyTheUser";

        #endregion

        /// <summary>
        /// Receives text and forwards it.
        /// </summary>
        /// <param name="messageText">Data.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async void CheckAnswerAsync(string messageText, long chatId, CancellationToken cancellationToken)
        {
            vBotControllerBase(chatId, cancellationToken);

        #region validations answer

        start:
            if (messageText == "/start")
            {
                SetDictionary<byte>(chatId, 0, NavigationDictionary);
                await PrintMessage("Привіт", chatId);
                await PrintMessage("Я твій помічник у двох запиатаннях:", chatId);
                await PrintMessage("Нагадування та період овуляції", chatId);
                await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
            }
            else if (messageText == ReminderCommands.Нагадування.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary<byte>(chatId, 1, NavigationDictionary);
            }
            else if (messageText == ReminderCommands.Додати.ToString())
            {
                SetDictionary(chatId, addReminder, InputTextDictionary);
                SetDictionary<byte>(chatId, 2, NavigationDictionary);
            }
            else if (messageText == ReminderCommands.Переглянути.ToString())
            {
                new ReminderController().DisplayReminder(chatId, cancellationToken);
            }
            else if (messageText == PeriodCommands.Період.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Перiод.ToString(), NotifyTheUserCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary<byte>(chatId, 3, NavigationDictionary);
            }
            else if (messageText == PeriodCommands.Перiод.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary(chatId, "", InputTextDictionary);
                SetDictionary<byte>(chatId, 4, NavigationDictionary);
            }
            else if (messageText == PeriodCommands.Створити.ToString())
            {
                SetDictionary(chatId, addPeriod, InputTextDictionary);
            }
            else if (messageText == PeriodCommands.Проглянути.ToString())
            {
                SetDictionary(chatId, "", InputTextDictionary);
                new PeriodController().DisplayPeriod(chatId, cancellationToken);
            }
            else if (messageText == PeriodCommands.Розпочались.ToString())
            {
                new PeriodController().UpdatePeriodAsync(chatId);
            }
            else if (messageText == NotifyTheUserCommands.Користувачі.ToString())
            {
                await PrintMessage("Ця функція створена для того щоб повідомити твого друга/подругу, хлопця і т.д. у яких є телеграм, про те що утебе почались \"ці дні\",і якщо у тебе є їхній id, то тобі залишається лише ввести його та ім'я людини(будь-яке яке захочеш) і надіслати.", chatId);
                await PrintMessage("Дізнатись свій Chat Id можна на початку додатку при натисканні /start, або у цій вкладці натиснувши myId.", chatId);
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary<byte>(chatId, 4, NavigationDictionary);
            }
            else if (messageText == NotifyTheUserCommands.Добавити.ToString())
            {
                SetDictionary(chatId, addNotifyTheUser, InputTextDictionary);
            }
            else if (messageText == NotifyTheUserCommands.Продивитись.ToString())
            {
                new NotifyTheUserController().DispalyNotifyTheUser(chatId, cancellationToken);
            }
            else if (messageText == GeneralCommands.Назад.ToString())
            {
                SetDictionary(chatId, "", InputTextDictionary);

                if (!NavigationDictionary.ContainsKey(chatId))
                {
                    await PrintMessage("Щось не так.\nНатискай /start.", chatId);
                    return;
                }


                switch (NavigationDictionary[chatId])
                {
                    case 1:
                        await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                        return;
                    case 2:
                        messageText = ReminderCommands.Нагадування.ToString();
                        goto start;
                    case 3:
                        await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                        return;
                    case 4:
                        messageText = PeriodCommands.Період.ToString();
                        goto start;
                    case 5:
                        messageText = PeriodCommands.Перiод.ToString();
                        goto start;
                    case 6:
                        await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                        SetDictionary<byte>(chatId, 4, NavigationDictionary);
                        return;
                    default:
                        await PrintMessage("Щось не так.\nНатискай /start.", chatId);
                        break;
                }
            }
            else if (messageText == GeneralCommands.myId.ToString())
            {
                await PrintMessage($"Id твого чату:", chatId);
                await PrintMessage($"{chatId}", chatId);
            }
            else
            {
                if(InputTextDictionary.ContainsKey(chatId))
                    InputTextDictionary[chatId] += messageText;
            }

            #endregion 

            if (InputTextDictionary.ContainsKey(chatId))
                CheckCommandsAsync(InputTextDictionary[chatId], chatId, cancellationToken);
        }

        /// <summary>
        /// Receives data and forwards it further for processing.
        /// </summary>
        /// <param name="text">Data.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="token">Token.</param>
        private void CheckCommandsAsync(string text, long chatId, CancellationToken token)
        {
            if (text.Contains(addReminder))
            {
                reminderController.AddReminder(chatId, text, out string inputText, token);
                InputTextDictionary[chatId] = inputText;
            }
            else if (text.Contains(addPeriod))
            {
                periodController.AddPeriod(chatId, text, out string inputText, out byte button, token);
                InputTextDictionary[chatId] = inputText;
                NavigationDictionary[chatId] = button;
            }
            else if (text.Contains(addNotifyTheUser))
            {
                notifyTheUserController.AddNotifyTheUser(chatId, text, out string inputText, out byte button, token);
                InputTextDictionary[chatId] = inputText;
                NavigationDictionary[chatId] = button;
            }
        }

        /// <summary>
        /// Receives the callback and forwards it.
        /// </summary>
        /// <param name="callbackQuery">Callback</param>
        public void CallbackQueryAsync(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data.Contains(CallbackQueryCommands.saveReminder.ToString()))
            {
                reminderController.SaveReminderAsync(callbackQuery.Message.Chat.Id);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteReminder.ToString()))
            {
                new ReminderController().DeleteReminder(callbackQuery);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.savePeriod.ToString()))
            {
                periodController.SavePeriodAsync(callbackQuery.Message.Chat.Id);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deletePeriod.ToString()))
            {
                new PeriodController().DeletePeriod(callbackQuery);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteNotifyTheUser.ToString()))
            {
                notifyTheUserController.DeleteNotifyTheUser(callbackQuery);
            }
        }
    }
}

