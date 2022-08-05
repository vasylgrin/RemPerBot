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
        /// Receives input data and passes it to SaveReminderAsync.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="text">Input data.</param>
        /// <param name="outputText">Output data.</param>
        public void AddReminder(long chatId, string text, out string outputText, CancellationToken token)
        {
            const string addReminder = "addReminder ";
            const string addReminderTopic = "addReminderTopic ";
            const string addReminderDateTime = "addReminderDateTime ";
            string output = "addReminder ";

            Task AddReminder = Task.Run(async () =>
            {
                string[] word = text.Trim().Split(new char[] { ' ' });
                word[0] += " ";

                if (word[0]==addReminder)
                {
                    await PrintKeyboard("Введи тему нагадування\nНаприклад: Сходити в магазин.", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    output = addReminderTopic;
                }
                else if (word[0] == addReminderTopic)
                {
                    text = text.Replace(addReminderTopic, "");

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        await PrintMessage("Введіть дату\nНаприклад: 17.09.2021 19:00:00", chatId);
                        SetDictionary(chatId, text, TopicDictionary);
                        output = addReminderDateTime;
                    }
                    else
                        output = addReminderTopic;
                }
                else if (word[0] == addReminderDateTime)
                {
                    text = text.Replace(addReminderDateTime, "");
                    if (DateTime.TryParse(text, out DateTime dateTime))
                    {
                        if (dateTime < DateTime.Now)
                        {
                            await PrintMessage("Дата не може бути з минулого...", chatId);
                            output = addReminderDateTime;
                            return;
                        }
                        else
                        {
                            SetDictionary(chatId, dateTime, DateTimeDictionary);
                            await PrintInline($"Тема: {TopicDictionary[chatId]}\nДата нагадування: {DateTimeDictionary[chatId]}", chatId, SetupInLine(CallbackQueryCommands.Зберегти.ToString(), CallbackQueryCommands.saveReminder.ToString()), token);
                            await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                            output = "";
                        }
                    }
                }             
            });

            AddReminder.Wait();
            
            outputText = output;
        }

        /// <summary>
        /// We create a new reminder and save it to the database.
        /// </summary>
        /// <param name="chatId">Id chat.</param>
        /// <param name="topic">Reminder topic.</param>
        /// <param name="dateTime">Reminder date.</param>
        public async void SaveReminderAsync(long chatId)
        {          
            if (new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).SaveDB(new Reminder(chatId, TopicDictionary[chatId], DateTimeDictionary[chatId])))
            {
                TopicDictionary.Remove(chatId);
                DateTimeDictionary.Remove(chatId);
                await PrintMessage("Нагадування збережено.", chatId);
            }
            else
                await PrintMessage("Нагадування не збережено.", chatId);
        }

        /// <summary>
        /// Gets valid reminders and displays them to the user, then deletes them.
        /// </summary>
        public void CheckReminders()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    // We receive all reminders.
                    new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).LoadDB(out List<Reminder> reminders);

                    // We choose whether there are reminders that need to happen,and display them to user.
                    reminders.Where(x => x.DateTime <= DateTime.Now).Where(p=>reminders.Count != 0).ToList().ForEach(async p=>
                    {
                        await PrintMessage($"Ваше нагадування: \nТема:{p.Topic}", p.ChatId);
                        new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).RemoveDB(p);
                    });

                    // 60 second delay.
                    Task.Delay(60000).Wait();
                }
            });
        }

        /// <summary>
        /// Displays all user reminders.
        /// </summary>
        /// <param name="chatId">Chat Id</param>
        /// <param name="cancellationToken">Token.</param>
        public async void DisplayReminder(long chatId,CancellationToken cancellationToken)
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
                    await PrintInline($"{rem.Topic} {rem.DateTime}", chatId, SetupInLine(CallbackQueryCommands.Видалити.ToString(), CallbackQueryCommands.deleteReminder.ToString() + rem.Id), cancellationToken);
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
                    return;
                }
            }
        }
    }
}
