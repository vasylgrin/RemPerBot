using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.Controller;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class NotifyTheUserController : BotControllerBase
    {
        Dictionary<long, long> ChatIdAddedDictionary = new();
        Dictionary<long, string> UsernameAddedDictionary = new();
        Dictionary<long, int> ClearDictionaryUser = new();
        ObjectControllerBase objectControllerBase = new ObjectControllerBase();

        public bool Add(string inputText, AddNotifyTheUserEnum addNotifyTheUserEnum, Update update, long chatId, CancellationToken token, out Dictionary<long, AddNotifyTheUserEnum> AddNotifyTheUserDictionary, out Dictionary<long, NavigationEnum> NavigationDictionary)
        {
            bool isOK = false;

            AddNotifyTheUserDictionary = new Dictionary<long, AddNotifyTheUserEnum>();
            NavigationDictionary = new Dictionary<long, NavigationEnum>();

            if (CheckForSaveNotifyTheUser(chatId, inputText, addNotifyTheUserEnum, out AddNotifyTheUserEnum addNotifyTheUserEnumOut, out NavigationEnum navigationEnum, token))
            {
                isOK = true;
            }

            objectControllerBase.SetDictionary(chatId, addNotifyTheUserEnumOut, AddNotifyTheUserDictionary);
            objectControllerBase.SetDictionary(chatId, navigationEnum, NavigationDictionary);
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
                        if (text.Length >= 6 && text.Length <= 10)
                        {
                            if (isExistUser(chatIdAdded))
                            {
                                objectControllerBase.SetDictionary(chatId, chatIdAdded, ChatIdAddedDictionary);

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

                    objectControllerBase.SetDictionary(chatId, text, UsernameAddedDictionary);

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
        public bool Save(long chatId, out int notifyTheUserId, long chatIdAddedDictionaryValue = 0, string usernameAddedDictionaryValue = default)
        {
            bool isOk = false;
            int ntu = 0;
            
            long chatIdAdded = chatId;

            chatId = ChatIdAddedDictionary.FirstOrDefault(x => x.Value == chatId).Key;

            if (chatIdAddedDictionaryValue == 0 && string.IsNullOrWhiteSpace(usernameAddedDictionaryValue))
            {
                ChatIdAddedDictionary.TryGetValue(chatId, out chatIdAddedDictionaryValue);
                UsernameAddedDictionary.TryGetValue(chatId, out usernameAddedDictionaryValue);
            }

            if (isExistPeriod(chatId, out List<Period> period))
            {
                if (chatIdAddedDictionaryValue != 0 && !string.IsNullOrWhiteSpace(usernameAddedDictionaryValue))
                {
                    if (new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).Save(new NotifyTheUser(chatId, chatIdAddedDictionaryValue, usernameAddedDictionaryValue, period.First().Id)))
                    {
                        ntu = new DataBaseControllerBase<NotifyTheUser>(new DataBaseContextForPeriod()).Load().Where(ntu => ntu.ChatId == chatId).Last().Id;

                        ChatIdAddedDictionary.Remove(chatId);
                        UsernameAddedDictionary.Remove(chatId);
                        isOk = true;
                    }
                }
            }

            notifyTheUserId = ntu;
            return isOk;
        }

        /// <summary>
        /// Checks whether a period exists, if it exists, we get it in the list.
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        private bool isExistPeriod(long chatId, out List<Period> periods)
        {
            periods = new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load().Where(p => p.ChatId == chatId).ToList();
            
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
            ChatIdAddedDictionary.TryGetValue(chatId, out long temp);
            ChatIdAddedDictionary.TryGetValue(chatId, out long chatIdAdded);
            if (temp != 0)
            {
                await PrintInline($"Запрошення на приймання повідомлень від користувача {update?.CallbackQuery?.Message?.Chat.Username}", chatIdAdded, SetupInLine(CallbackQueryCommands.Підтвердити.ToString(), CallbackTextSendInvite));
            }
        }

        public void ClearDictionary()
        {
            foreach (var item in ClearDictionaryUser)
            {
                if (item.Value >= 5 && item.Value != 0)
                {
                    ClearDictionaryUser[item.Key] = item.Value - 1;
                }
                else if (item.Value == 0)
                {
                    ChatIdAddedDictionary.Remove(item.Key);
                    UsernameAddedDictionary.Remove(item.Key);
                    ClearDictionaryUser.Remove(item.Key);
                }
            }
        }

    }
}
