using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class ReminderController : BotControllerBase
    {
        public ReminderController()
        {

        }

        /// <summary>
        /// Receives input data and passes it to SaveReminderAsync.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="text">Input data.</param>
        /// <param name="outputText">Output data.</param>
        public void AddReminder(long chatId, string text, out string outputText, CancellationToken token)
        {
            string input = "addReminder";

            text = text.Replace("addReminder", "");

            string[] word = text.Trim().Split(new char[] { ' ' });

            Task.Factory.StartNew(async () =>
            {
                if (word.Length > 1)
                {
                    if (DateTime.TryParse(word.Last().Replace("_", " "), out DateTime dateTime))
                    {
                        text = text.Replace(word.Last(), "");

                        #region check for null

                        if (chatId < 0)
                        {
                            await PrintMessage("Некоректний id чату...", chatId);
                            input = "addReminder";
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            await PrintMessage("Тема не може бути пустою...", chatId);
                            input = "addReminder";
                            return;
                        }
                        if (dateTime < DateTime.Now)
                        {
                            await PrintMessage("Дата не може бути з минулого...", chatId);
                            input = "addReminder";
                            return;
                        }

                        #endregion
                        
                        SaveReminderAsync(chatId, text, dateTime);
                        await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                        input = "";
                    }
                    else
                    {
                        await PrintKeyboard("Введи тему та дату нагадування\nПриклад: Сходити в магазин 20.10.2023_10:30:00", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    }
                }
                else
                {
                    await PrintKeyboard("Введи тему та дату нагадування\nПриклад: Сходити в магазин 20.10.2023_10:30:00", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                }
            });         

            outputText = input;
        }

        /// <summary>
        /// We create a new reminder and save it to the database.
        /// </summary>
        /// <param name="chatId">Id chat.</param>
        /// <param name="topic">Reminder topic.</param>
        /// <param name="dateTime">Reminder date.</param>
        private async void SaveReminderAsync(long chatId, string topic, DateTime dateTime)
        {          
            if (new DataBaseControllerBase<Reminder>(new DataBaseContextForReminder()).SaveDB(new Reminder(chatId, topic, dateTime)))
                await PrintMessage("Нагадування збережено.", chatId);
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
