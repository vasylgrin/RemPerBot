using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    internal class NotifyTheUserController : BotControllerBase
    {
        Dictionary<long, long> ChatIdAddedDictionary = new();
        Dictionary<long, string> UsernameAddedDictionary = new();

        public bool AddNotifyTheUser(string inputText, AddNotifyTheUserEnum addNotifyTheUserEnum, Update update, long chatId, CancellationToken token,
            out Dictionary<long, AddNotifyTheUserEnum> AddNotifyTheUserDictionary,
            out Dictionary<long, NavigationEnum> NavigationDictionary,
            out Dictionary<long, string> InputTextDictionary)
        {
            bool isOK = false;

            AddNotifyTheUserDictionary = new Dictionary<long, AddNotifyTheUserEnum>();
            NavigationDictionary = new Dictionary<long, NavigationEnum>();
            InputTextDictionary = new Dictionary<long, string>();

            AddNotifyTheUserEnum addNotifyTheUserEnumOut = AddNotifyTheUserEnum.addNotifyTheUser;
            NavigationEnum navigationEnum = NavigationEnum.addNotifyTheUser;

            Task task = Task.Run(async () =>
            {
                if (CheckForSaveNotifyTheUser(chatId, inputText, addNotifyTheUserEnum, out addNotifyTheUserEnumOut, out navigationEnum, token))
                {
                    isOK = true;
                }
            });

            task.Wait();

            if(isOK)
                SetDictionary(chatId, "", InputTextDictionary);


            SetDictionary(chatId, addNotifyTheUserEnumOut, AddNotifyTheUserDictionary);
            SetDictionary(chatId, navigationEnum, NavigationDictionary);
            return isOK;
        }



        /// <summary>
        /// Gets the input and stores the user.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="text">Data.</param>
        /// <param name="outputText">Output Data.</param>
        /// <param name="navigation">Navigation bitton.</param>
        /// <param name="token">Token.</param>
        private bool CheckForSaveNotifyTheUser(long chatId, string text, AddNotifyTheUserEnum addNotifyTheUserEnum,out AddNotifyTheUserEnum addNotifyTheUserEnumOut, out NavigationEnum navigationEnum, CancellationToken token)
        {
            #region Variables

            NavigationEnum navEnum = NavigationEnum.addNotifyTheUser;
            AddNotifyTheUserEnum addNTUEnum = AddNotifyTheUserEnum.addNotifyTheUser;
            bool isOK = false;

            List<Period> periods = new();          
            
            #endregion

            Task task = Task.Run(async () =>
            {
                if(addNotifyTheUserEnum == AddNotifyTheUserEnum.addNotifyTheUser)
                {
                    if(!isExistPeriod(chatId, out periods))
                    {
                        await PrintMessage("Твій цикл не знайдено, саме час його створити.", chatId);
                        return;
                    }
                    await PrintKeyboard("Введи чат Id користувача якого хочеш добавити\nПриклад: 000000001", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    addNTUEnum = AddNotifyTheUserEnum.addNotifyTheUserChatId;
                    return;
                }
                else if (addNotifyTheUserEnum == AddNotifyTheUserEnum.addNotifyTheUserChatId)
                {
                    addNTUEnum = AddNotifyTheUserEnum.addNotifyTheUserChatId;
                    
                    text = text.Trim().Replace(AddNotifyTheUserEnum.addNotifyTheUserChatId.ToString(), "");
                    
                    if(long.TryParse(text, out long chatIdAdded))
                    {
                        if (text.Length == 9)
                        {
                            if (isExistUser(chatIdAdded))
                            {
                                SetDictionary(chatId, chatIdAdded, ChatIdAddedDictionary);

                                addNTUEnum = AddNotifyTheUserEnum.addNotifyTheUserName;
                                await PrintMessage("Введи Ім'я користувача." +
                                    "\nНаприклад: Котик\nP.S Це ім'я будеш бачити тільки ти.", chatId);
                            }
                            else
                                await PrintMessage("Ви вже добавили цього користувача.", chatId);
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
                else if (addNotifyTheUserEnum == AddNotifyTheUserEnum.addNotifyTheUserName)
                {
                    addNTUEnum = AddNotifyTheUserEnum.addNotifyTheUserName;
                    
                    text = text.Trim().Replace(AddNotifyTheUserEnum.addNotifyTheUserName.ToString(), "");
                    
                    if (string.IsNullOrWhiteSpace(text))
                    {
                        await PrintMessage("Ім'я не може бути пустим.", chatId);
                    }

                    SetDictionary(chatId, text, UsernameAddedDictionary);

                    //await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), token);
                    navEnum = NavigationEnum.PeriodMenu;
                    isOK = true;
                }
            });
            
            task.Wait();
            
            addNotifyTheUserEnumOut = addNTUEnum;
            navigationEnum = navEnum;
            return isOK;
        }

        public async Task DisplayCurrentNotifyTheUser(long chatId, string callbackName) => await PrintInline($"Дані користувача:\nChat Id:{ChatIdAddedDictionary[chatId]}\nІм'я: {UsernameAddedDictionary[chatId]}", chatId, SetupInLine(CallbackQueryCommands.Запросити.ToString(), callbackName));



        /// <summary>
        /// Checks for null and saves the user.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="chatIdAdded">Chat ID of the user we are adding.</param>
        /// <param name="name">Username.</param>
        /// <returns></returns>
        public async Task<bool> SaveNotifyTheUserAsync(long chatId)
        {
            long chatIdAddedDictionaryValue = 0;
            string usernameAddedDictionaryValue = "";

            if(isExistPeriod(chatId, out List<Period> period))
            {
                if (GetDictionary(chatId, ChatIdAddedDictionary, out chatIdAddedDictionaryValue)
                && GetDictionary(chatId, UsernameAddedDictionary, out usernameAddedDictionaryValue))
                {
                    if (new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).SaveDB(new NotifyTheUser(chatId, chatIdAddedDictionaryValue, usernameAddedDictionaryValue, period.First().Id)))
                    {
                        await PrintMessage("Користувач успішно добавлений.", chatId);
                        ChatIdAddedDictionary.Remove(chatId);
                        UsernameAddedDictionary.Remove(chatId);
                        return true;
                    }
                    else
                        await PrintMessage("Користувача не добавлено.", chatId);
                }
            }
            return false;
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

        private bool isExistUser(long chatId)
        {
            bool isOK = true;
            List<Period> Periods = new();
            
            using (DataBaseContextForPeriod db = new())
            {
                Periods = db.Periods.Include(n => n.NotifyTheUser).ToList();
            }

            Periods.Where(p => p.ChatId == chatId).ToList().ForEach(p =>
            {
                p.NotifyTheUser.Where(n => n.ChatIdAdded == chatId).ToList().ForEach(async p =>
                {
                    isOK = false;
                });
            });

            return isOK;
        }

        public async Task SendInvite(long chatId, Update update, string CallbackTextSendInvite)
        {            
            if(GetDictionary(chatId, ChatIdAddedDictionary) != 0)
            {
                await PrintInline($"Запрошення на приймання повідомлень від користувача {update?.CallbackQuery?.Message?.Chat.Username}", GetDictionary(chatId, ChatIdAddedDictionary), SetupInLine(CallbackQueryCommands.Підтвердити.ToString(), CallbackTextSendInvite));
            }
        }


        /// <summary>
        /// Displays all users.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async Task DispalyNotifyTheUser(long chatId,CancellationToken cancellationToken)
        {
            new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).LoadDB(out List<NotifyTheUser> notifyTheUser);

            notifyTheUser = notifyTheUser.Where(notify => notify.ChatId == chatId).ToList();

            if (notifyTheUser.Count != 0)
            {
                notifyTheUser.ForEach(async n =>
                {
                    await PrintInline($"{n.Name} {n.ChatIdAdded}", chatId, SetupInLine("Видалити", "deleteNotifyTheUser" + n.Id));
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
