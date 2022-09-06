using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class ReminderController : BotControllerBase
    {
        Dictionary<long, string> TopicDictionary = new();
        Dictionary<long, DateTime> DateTimeDictionary = new();
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="addReminderEnum"></param>
        /// <param name="chatId"></param>
        /// <param name="token"></param>
        /// <param name="AddReminderDictionary"></param>
        /// <param name="NavigationDictionary"></param>
        /// <param name="InputTextDictionary"></param>
        /// <returns></returns>
        public bool AddReminder(string inputText, AddReminderEnum addReminderEnum, long chatId, CancellationToken token, out Dictionary<long, AddReminderEnum> AddReminderDictionary, out Dictionary<long, NavigationEnum> NavigationDictionary, out Dictionary<long, string> InputTextDictionary)
        {
            bool isOK = false;
            
            AddReminderEnum addReminderEnumOut = AddReminderEnum.addReminder;
            NavigationEnum navigationEnumOut = NavigationEnum.AddReminder;
            
            AddReminderDictionary = new Dictionary<long, AddReminderEnum>();
            NavigationDictionary = new Dictionary<long, NavigationEnum>();
            InputTextDictionary = new Dictionary<long, string>();

            Task task = Task.Run(async () =>
            {
                if (CheckForSaveReminder(chatId, inputText, addReminderEnum, out addReminderEnumOut, out navigationEnumOut, token))
                {
                    await DisplayCurrentReminder(chatId, CallbackQueryCommands.saveReminder.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                    isOK = true;
                }
            });

            task.Wait();
            
            if(isOK)
                SetDictionary(chatId, "", InputTextDictionary);


            SetDictionary(chatId, addReminderEnumOut, AddReminderDictionary);
            SetDictionary(chatId, navigationEnumOut, NavigationDictionary);
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
                        SetDictionary(chatId, text, TopicDictionary);
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
                            SetDictionary(chatId, dateTime, DateTimeDictionary);
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
        public async Task<bool> SaveReminderAsync(long chatId)
        {
            string topicDictionaryValue = "";
            DateTime dateTimeDictionaryValue = DateTime.MinValue;

            if (GetDictionary(chatId, TopicDictionary, out topicDictionaryValue)
                && GetDictionary(chatId, DateTimeDictionary, out dateTimeDictionaryValue))
            {
                if (new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).SaveDB(new Reminder(chatId, topicDictionaryValue, dateTimeDictionaryValue)))
                {
                    TopicDictionary.Remove(chatId);
                    DateTimeDictionary.Remove(chatId);
                    await PrintMessage("Нагадування збережено.", chatId);
                    return true;
                }
                else
                    await PrintMessage("Нагадування не збережено.", chatId);
            }
            return false;
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
                    // We receive all reminders.
                    new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).LoadDB(out List<Reminder> reminders);

                    // We choose whether there are reminders that need to happen,and display them to user.                   
                    foreach(var reminder in reminders)
                    {
                        if(reminder.DateTime <= DateTime.Now)
                        {
                            await PrintMessage($"Ваше нагадування: \nТема:{reminder.Topic}", reminder.ChatId);
                            new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).RemoveDB(reminder);
                        }
                    }
                    Task.Delay(1000).Wait();
                }
            });
        }

        /// <summary>
        /// Displays all user reminders.
        /// </summary>
        /// <param name="chatId">Chat Id</param>
        /// <param name="cancellationToken">Token.</param>
        public async Task DisplayReminder(long chatId,CancellationToken cancellationToken)
        {
            // Receives all reminders.
            new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).LoadDB(out List<Reminder> reminders);

            // Selects all user reminders by chatId.
            reminders = reminders.Where(rem => rem.ChatId == chatId).ToList();
            
            // Check for null.
            if (reminders.Count != 0)
            {
                // Displays a reminder.
                reminders.ForEach(async rem =>
                {
                    await PrintInline($"{rem.Topic} {rem.DateTime}", chatId, SetupInLine(CallbackQueryCommands.Видалити.ToString(), CallbackQueryCommands.deleteReminder.ToString() + rem.Id));
                });
                return;
            }
            else
            {
                await PrintMessage("У тебе немає нагадувань, час їх створити.", chatId);
                return;
            }
        }

        /// <summary>
        /// Gets the callback and removes the reminder.
        /// </summary>
        /// <param name="callbackQuery">Callback.</param>
        public async void DeleteReminder(CallbackQuery callbackQuery)
        {
            string messageText = callbackQuery.Data.Replace(CallbackQueryCommands.deleteReminder.ToString(), "");
            
            new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).LoadDB(out List<Reminder> reminders);
            
            foreach (var reminder in reminders)
            {
                if (reminder.Id == Convert.ToInt32(messageText))
                {
                    if (new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).RemoveDB(reminder))
                        await PrintMessage("Нагадування видалено.", callbackQuery.Message.Chat.Id);
                    else
                        await PrintMessage("Нагадування не видалено.", callbackQuery.Message.Chat.Id);
                }
            }
        }
    }
}
