using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.Controller;

namespace MySuperUniversalBot_BL.Controller
{
    public class ReminderController : BotControllerBase
    {
        #region Variable

        Dictionary<long, string> TopicDictionary = new();
        Dictionary<long, DateTime> DateTimeDictionary = new();
        Dictionary<long, int> ClearDictionaryUser = new();
        ObjectControllerBase objectControllerBase = new();

        #endregion

        public bool Add(string inputText, AddReminderEnum addReminderEnum, long chatId, CancellationToken token, out Dictionary<long, AddReminderEnum> AddReminderDictionary, out Dictionary<long, NavigationEnum> NavigationDictionary)
        {
            bool isOK = false;

            AddReminderDictionary = new Dictionary<long, AddReminderEnum>();
            NavigationDictionary = new Dictionary<long, NavigationEnum>();

            if (CheckForSaveReminder(chatId, inputText, addReminderEnum, out AddReminderEnum addReminderEnumOut, out NavigationEnum navigationEnumOut, token))
            {
                isOK = true;
            }

            objectControllerBase.SetDictionary(chatId, addReminderEnumOut, AddReminderDictionary);
            objectControllerBase.SetDictionary(chatId, navigationEnumOut, NavigationDictionary);
            return isOK;
        }

        /// <summary>
        /// Receives input data and passes it to SaveReminderAsync.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="text">Input data.</param>
        /// <param name="outputText">Output data.</param>
        private bool CheckForSaveReminder(long chatId, string text, AddReminderEnum addReminderEnum, out AddReminderEnum addReminderEnumOut, out NavigationEnum navigationEnum, CancellationToken token)
        {
            #region Varibales

            AddReminderEnum addRemEnum = AddReminderEnum.addReminder;
            NavigationEnum navEnum = NavigationEnum.AddReminder;
            bool isOK = false;

            #endregion

            Task AddReminder = Task.Run(async () =>
            {
                if (addReminderEnum == AddReminderEnum.addReminder)
                {
                    await PrintKeyboard("Введи тему нагадування\nНаприклад: Сходити в магазин.", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    addRemEnum = AddReminderEnum.addReminderTopic;
                }
                else if (addReminderEnum == AddReminderEnum.addReminderTopic)
                {
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        await PrintMessage($"Введіть дату\nНаприклад: {DateTime.Now.AddMinutes(3)}", chatId);
                        objectControllerBase.SetDictionary(chatId, text, TopicDictionary);
                        ClearDictionaryUser.Add(chatId, 5);
                        addRemEnum = AddReminderEnum.addReminderDateTime;
                    }
                    else
                    {
                        addRemEnum = AddReminderEnum.addReminderTopic;
                        await PrintMessage("Щось не так з текстом...", chatId);
                    }
                }
                else if (addReminderEnum == AddReminderEnum.addReminderDateTime)
                {
                    text = text.Replace(AddReminderEnum.addReminderDateTime.ToString(), "");

                    if (DateTime.TryParse(text, out DateTime dateTime))
                    {
                        if (dateTime < DateTime.Now)
                        {
                            await PrintMessage($"Дата не може бути з минулого... Введіть коректні данні." +
                                $"\nНаприклад: {DateTime.Now.AddMinutes(3)}.", chatId);
                            addRemEnum = AddReminderEnum.addReminderDateTime;
                            return;
                        }
                        else
                        {
                            objectControllerBase.SetDictionary(chatId, dateTime, DateTimeDictionary);
                            addRemEnum = AddReminderEnum.empty;
                            navEnum = NavigationEnum.ReminderMenu;
                            isOK = true;
                        }
                    }
                    else
                    {
                        addRemEnum = AddReminderEnum.addReminderDateTime;
                        await PrintMessage($"Введіть правильний формат дати.\nНаприклад: {DateTime.Now.AddMinutes(3)}.", chatId);
                    }
                }
            });

            AddReminder.Wait();

            addReminderEnumOut = addRemEnum;
            navigationEnum = navEnum;
            return isOK;
        }

        /// <summary>
        /// Displays a reminder before saving it.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="callbackName">Сallback to save the reminder.</param>
        /// <returns></returns>
        public async Task DisplayCurrentReminder(long chatId, string callbackName) => await PrintInline($"Тема: {TopicDictionary[chatId]}\nДата нагадування: {DateTimeDictionary[chatId]}", chatId, SetupInLine(CallbackQueryCommands.Зберегти.ToString(), callbackName));

        /// <summary>
        /// We create a new reminder and save it to the database.
        /// </summary>
        /// <param name="chatId">Id chat.</param>
        /// <param name="topic">Reminder topic.</param>
        /// <param name="dateTime">Reminder date.</param>
        public bool Save(long chatId, out int reminderId, string topicDictionaryValue = default, DateTime dateTimeDictionaryValue = default)
        {
            bool isOk = false;
            int remId = 0;

            if (string.IsNullOrWhiteSpace(topicDictionaryValue) && dateTimeDictionaryValue == DateTime.MinValue)
            {
                TopicDictionary.TryGetValue(chatId, out topicDictionaryValue);
                DateTimeDictionary.TryGetValue(chatId, out dateTimeDictionaryValue);
            }

            if (!string.IsNullOrWhiteSpace(topicDictionaryValue) && dateTimeDictionaryValue != DateTime.MinValue)
            {
                if (new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).Save(new Reminder(chatId, topicDictionaryValue, dateTimeDictionaryValue)))
                {
                    remId = new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).Load().Where(r => r.ChatId == chatId).Last().Id;

                    TopicDictionary.Remove(chatId);
                    DateTimeDictionary.Remove(chatId);
                    isOk = true;
                }
            }

            reminderId = remId;
            return isOk;
        }

        /// <summary>
        /// Gets valid reminders and displays them to the user, then deletes them.
        /// </summary>
        public void CheckReminders()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).Load().Where(rem => rem.DateTime <= DateTime.Now).ToList().ForEach(async rem =>
                    {
                        await PrintMessage($"Ваше нагадування: \nТема:{rem.Topic}", rem.ChatId);
                        new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).Remove(rem);
                    });

                    Task.Delay(1000).Wait();
                }
            });
        }

        public void ClearDictionary()
        {
            foreach (var item in ClearDictionaryUser)
            {
                if (item.Value <= 5 && item.Value != 0)
                {
                    ClearDictionaryUser[item.Key] = item.Value - 1;
                }
                else if (item.Value == 0)
                {
                    TopicDictionary.Remove(item.Key);
                    DateTimeDictionary.Remove(item.Key);
                    ClearDictionaryUser.Remove(item.Key);
                }
            }
        }
    }
}
