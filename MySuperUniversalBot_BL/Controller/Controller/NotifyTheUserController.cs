using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    internal class NotifyTheUserController : BotControllerBase
    {
        /// <summary>
        /// Gets the input and stores the user.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="text">Data.</param>
        /// <param name="outputText">Output Data.</param>
        /// <param name="button">Navigation bitton.</param>
        /// <param name="token">Token.</param>
        public void AddNotifyTheUser(long chatId, string text, out string outputText, out int button, CancellationToken token)
        {
            int but = 0;
            string output = "addPersonForPeriod";
            text = text.Replace("addPersonForPeriod", "");

            Task task = Task.Run(async () =>
            {
                string[] word = text.Split(new char[] { ' ' });

                if (CheckForNull(word, chatId, out long chatIdAdded, out but, out List<Period> periods, token))
                {
                    but = 6;
                    output = "";
                    await SaveNotifyTheUserAsync(chatId,chatIdAdded,word[1],periods);
                }
                else
                {
                    word = null;
                }
            });
            
            task.Wait();
            outputText = output;
            button = but;
        }

        /// <summary>
        /// Checks for null and returns false if something is wrong.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="chatId">Chat id user.</param>
        /// <param name="ChatIdAdded">Chat Id of the user we are adding.</param>
        /// <param name="periods">User period.</param>
        /// <param name="token">Token.</param>
        /// <returns>It returns true if everything is fine and false if something is terribly wrong.</returns>
        private bool CheckForNull(string[] data, long chatId, out long ChatIdAdded, out int button, out List<Period> periods, CancellationToken token)
        {
            bool isNorm = false;
            List<Period> Periods = new();
            long chatIdAdded = 0;
            Task checkForNull = Task.Run(async () =>
            {
                if (isExistPeriod(chatId, out Periods))
                {
                    if (data.Length > 1)
                    {
                        if (long.TryParse(data[0], out chatIdAdded))
                        {
                            if (data[0].Length == 9)
                            {
                                isNorm = true;
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
                    else
                    {
                        await PrintKeyboard("Введи Chat Id та ім'я користувача якого хочеш добавити,\nНапркилад: 000000001 Котик", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    }
                }
                else
                {
                    // If the period is not created, then this will be done.
                    await PrintMessage("Твій цикл не знайдено, можливо саме час його створити?", chatId);
                }
            });
            
            checkForNull.Wait();
            ChatIdAdded = chatIdAdded;
            periods = Periods;
            button = 6;
            return isNorm;
        }

        /// <summary>
        /// Checks for null and saves the user.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="chatIdAdded">Chat ID of the user we are adding.</param>
        /// <param name="name">Username.</param>
        /// <returns></returns>
        private async Task SaveNotifyTheUserAsync(long chatId, long chatIdAdded, string name, List<Period> period)
        {
            if (new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).SaveDB(new NotifyTheUser(chatId, chatIdAdded, name, period.First().Id)))
                await PrintMessage("Користувач успішно добавлений.", chatId);
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
