using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    internal class NotifyTheUserController : BotControllerBase
    {
        Dictionary<long, long> ChatIdAddedDictionary = new();
        Dictionary<long, string> UsernameAddedDictionary = new();

        /// <summary>
        /// Gets the input and stores the user.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="text">Data.</param>
        /// <param name="outputText">Output Data.</param>
        /// <param name="button">Navigation bitton.</param>
        /// <param name="token">Token.</param>
        public void AddNotifyTheUser(long chatId, string text, out string outputText, out byte button, CancellationToken token)
        {
            byte but = 0;
            List<Period> periods = new();
            const string addNotifyTheUser = "addNotifyTheUser ";
            const string addNotifyTheUserChatId = "addNotifyTheUserChatId ";
            const string addNotifyTheUserName = "addNotifyTheUserName ";
            string output = "addPersonForPeriod";

            Task task = Task.Run(async () =>
            {
                string[] word = text.Trim().Split(new char[] { ' ' });
                word[0] += " ";

                if(word[0] == addNotifyTheUser)
                {
                    if(!isExistPeriod(chatId, out periods))
                    {
                        await PrintMessage("Твій цикл не знайдено, саме час його створити.", chatId);
                        return;
                    }
                    await PrintKeyboard("Введи чат Id користувача якого хочеш добавити\nПриклад: 000000001", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    output = addNotifyTheUserChatId;
                    return;
                }
                else if (word[0] == addNotifyTheUserChatId)
                {
                    output = addNotifyTheUserChatId;
                    text = text.Trim().Replace(addNotifyTheUserChatId,"");
                    if(long.TryParse(text, out long chatIdAdded))
                    {
                        if (text.Length == 9)
                        {
                            SetDictionary(chatId, chatIdAdded, ChatIdAddedDictionary);

                            output = addNotifyTheUserName;
                            await PrintMessage("Введи Ім'я користувача.", chatId);
                        }
                        else
                        {
                            await PrintMessage("Id складається 3 9 цифр, спробуй ще раз.", chatId);
                        }
                    }
                    else
                    {
                        await PrintMessage("Id має містити лише цифри, спробуй ще раз.", chatId);
                    }
                }
                else if (word[0] == addNotifyTheUserName)
                {
                    output = addNotifyTheUserName;
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await PrintMessage("Ім'я не може бути пустим.", chatId);
                    }

                    SetDictionary(chatId, text, UsernameAddedDictionary);

                    await PrintInline($"Дані користувача:\nChat Id:{ChatIdAddedDictionary[chatId]}\nІм'я: {UsernameAddedDictionary[chatId]}", chatId, SetupInLine(CallbackQueryCommands.Зберегти.ToString(), CallbackQueryCommands.saveNotifyTheUser.ToString()), token);
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), token);
                }
            });
            
            task.Wait();
            outputText = output;
            button = but;
        }

        /// <summary>
        /// Checks for null and saves the user.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="chatIdAdded">Chat ID of the user we are adding.</param>
        /// <param name="name">Username.</param>
        /// <returns></returns>
        public async Task SaveNotifyTheUserAsync(long chatId, long chatIdAdded, string name, List<Period> period)
        {
            if (new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).SaveDB(new NotifyTheUser(chatId, chatIdAdded, name, period.First().Id)))
            {
                await PrintMessage("Користувач успішно добавлений.", chatId);
                ChatIdAddedDictionary.Remove(chatId);
                UsernameAddedDictionary.Remove(chatId);
            }
            else
                await PrintMessage("Користувача не добавлено.", chatId);
        }

        /// <summary>
        /// Checks whether a period exists, if it exists, we get it in the list.
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        private bool isExistPeriod(long chatId, out List<Period> periods)
        {
            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).LoadDB(out periods);
            periods = periods.Where(p => p.ChatId == chatId).ToList();
            
            if(periods.Count == 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Displays all users.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async void DispalyNotifyTheUser(long chatId,CancellationToken cancellationToken)
        {
            new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).LoadDB(out List<NotifyTheUser> notifyTheUser);

            notifyTheUser = notifyTheUser.Where(notify => notify.ChatId == chatId).ToList();

            if (notifyTheUser.Count != 0)
            {
                notifyTheUser.ForEach(async n =>
                {
                    await PrintInline($"{n.Name} {n.ChatIdAdded}", chatId, SetupInLine("Видалити", "deleteNotifyTheUser" + n.Id), cancellationToken);
                });
                return;
            }
            else
            {
                await PrintMessage("У тебе немає доданих користувачів, мені здається самий час їх добавити.", chatId);
                return;
            }
        }
        
        /// <summary>
        /// Gets a callback and deletes a NotifyTheUser.
        /// </summary>
        /// <param name="callbackQuery"></param>
        public async void DeleteNotifyTheUser(CallbackQuery callbackQuery)
        {
            string messageText = callbackQuery.Data.Replace(CallbackQueryCommands.deleteNotifyTheUser.ToString(), "");
            DataBaseControllerBase<NotifyTheUser> dataBaseControllerBase = new(new DataBaseContextForPeriod());

            dataBaseControllerBase.LoadDB(out List<NotifyTheUser> notifyTheUsers);
            foreach (var notifyTheUser in notifyTheUsers)
            {
                if (notifyTheUser.Id == Convert.ToInt32(messageText))
                {
                    if (dataBaseControllerBase.RemoveDB(notifyTheUser))
                        await PrintMessage("Користувача видалено.", callbackQuery.Message.Chat.Id);
                    else
                        await PrintMessage("Користувача не видалено.", callbackQuery.Message.Chat.Id);
                    return;
                }
            }
        }
    }
}
