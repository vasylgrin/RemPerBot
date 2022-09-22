using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.Controller;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class BotController : BotControllerBase
    {
        #region Variable

        Dictionary<long, NavigationEnum> NavigationDictionary = new();       
        Dictionary<long, AddReminderEnum> AddReminderDictionary = new();
        Dictionary<long, AddPeriodEnum> AddPeriodDictionary = new();
        Dictionary<long, AddNotifyTheUserEnum> AddNotifyTheUserDictionary = new();
        
        Dictionary<long, string> InputTextDictionary = new();
        Dictionary<long, string> CurrentOperationDictionary = new();
        Dictionary<long, bool> IsStartAddedObjectDictionary = new();


        ReminderController reminderController = new();
        PeriodController periodController = new();
        NotifyTheUserController notifyTheUserController = new();
        ObjectControllerBase objectControllerBase = new();

        #endregion

        /// <summary>
        /// Receives text and forwards it.
        /// </summary>
        /// <param name="messageText">Data.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async Task CheckAnswerAsync(string messageText, long chatId, Update update, CancellationToken cancellationToken)
        {
            objectControllerBase.SetsTheInitialValue(chatId, InputTextDictionary, CurrentOperationDictionary, IsStartAddedObjectDictionary);
        #region validations answer

        start:
            if (messageText == "/start")
            {
                //SetDictionary<int>(chatId, NavigationEnum.ReminderMenu, NavigationDictionary);
                await PrintInline("Привіт, мене звати RemPer.", chatId, SetupInLine("Пройти туторіал", CallbackQueryCommands.tutorialPeriod.ToString()));
                await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
            }
            else if (messageText == GeneralCommands.Назад.ToString())
            {
                if (BackButton(chatId, out messageText, cancellationToken))
                {
                    goto start;
                }
            }
            else if (messageText == ReminderCommands.Нагадування.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                objectControllerBase.SetDictionary(chatId, NavigationEnum.ReminderMenu, NavigationDictionary);
            }
            else if (messageText == ReminderCommands.Додати.ToString() || ReminderCommands.Додати.ToString() == CurrentOperationDictionary[chatId])
            {
                if (await AddObject(chatId, messageText, ReminderCommands.Додати.ToString(), ReminderCommands.Додати.ToString(), AddReminder, cancellationToken, update))
                {
                    await reminderController.DisplayCurrentReminder(chatId, CallbackQueryCommands.saveReminder.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                    InputTextDictionary.Add(chatId, "");
                }
            }
            else if (messageText == ReminderCommands.Переглянути.ToString())
            {
                objectControllerBase.Display<Reminder>(chatId, "У тебе немає нагадувань, час їх створити.", CallbackQueryCommands.deleteReminder, new DataBaseContextForReminder());
            }
            else if (messageText == PeriodCommands.Період.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Перiод.ToString(), NotifyTheUserCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                objectControllerBase.SetDictionary(chatId, NavigationEnum.StartPeriodMenu, NavigationDictionary);
            }
            else if (messageText == PeriodCommands.Перiод.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                objectControllerBase.SetDictionary(chatId, AddPeriodEnum.empty, AddPeriodDictionary);
                objectControllerBase.SetDictionary(chatId, NavigationEnum.PeriodMenu, NavigationDictionary);
            }
            else if (messageText == PeriodCommands.Створити.ToString() || PeriodCommands.Створити.ToString() == CurrentOperationDictionary[chatId])
            {
                if(await AddObject(chatId, messageText, PeriodCommands.Створити.ToString(), PeriodCommands.Створити.ToString(), AddPeriod, cancellationToken, update))
                {
                }
            }
            else if (messageText == PeriodCommands.Проглянути.ToString())
            {
                objectControllerBase.SetDictionary(chatId, AddPeriodEnum.empty, AddPeriodDictionary);
                objectControllerBase.Display<Period>(chatId, "У тебе немає створеного циклу, вже самий час його створити.", CallbackQueryCommands.deletePeriod, new DataBaseContextForPeriod());

            }
            else if (messageText == PeriodCommands.Розпочались.ToString())
            {
                new PeriodController().Update(chatId);
            }
            else if (messageText == NotifyTheUserCommands.Користувачі.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                objectControllerBase.SetDictionary(chatId, NavigationEnum.PeriodMenu, NavigationDictionary);
            }
            else if (messageText == NotifyTheUserCommands.Добавити.ToString() || NotifyTheUserCommands.Добавити.ToString() == CurrentOperationDictionary[chatId])
            {
                if(await AddObject(chatId, messageText, NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Добавити.ToString(), AddNotifyTheUser, cancellationToken, update))
                {
                    await notifyTheUserController.DisplayCurrentNotifyTheUser(chatId, CallbackQueryCommands.sendInviteNotifyTheUser.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                    InputTextDictionary.Add(chatId, "");
                }
            }
            else if (messageText == NotifyTheUserCommands.Продивитись.ToString())
            {
                objectControllerBase.Display<NotifyTheUser>(chatId, "У тебе немає доданих користувачів, мені здається самий час їх добавити.", CallbackQueryCommands.deleteNotifyTheUser,new DataBaseContextForPeriod());
            }
            else if (messageText == GeneralCommands.myId.ToString())
            {
                await PrintMessage($"Id твого чату:", chatId);
                await PrintMessage($"{chatId}", chatId);
            }
            else
            {
                await PrintMessage("Невідома команда...", chatId);
            }

            #endregion 
        }

        /// <summary>
        /// The method that returns to the previous page in the application.
        /// </summary>
        /// <param name="chatId">Chat Id.</param>
        /// <param name="outputText">Text for restart app.</param>
        /// <param name="cancellationToken">Token.</param>
        /// <returns></returns>
        private bool BackButton(long chatId, out string outputText, CancellationToken cancellationToken)
        {
            objectControllerBase.SetDictionary(chatId, AddReminderEnum.addReminder, AddReminderDictionary);
            objectControllerBase.SetDictionary(chatId, AddPeriodEnum.addPeriod, AddPeriodDictionary);
            objectControllerBase.SetDictionary(chatId, AddNotifyTheUserEnum.addNotifyTheUser, AddNotifyTheUserDictionary);
            objectControllerBase.SetDictionary(chatId, "", CurrentOperationDictionary);

            string outputTxt = "";
            bool isOk = false;

            Task task = Task.Run(async () =>
            {
                if (!NavigationDictionary.ContainsKey(chatId))
                {
                    await PrintMessage("Щось не так.\nНатискай /start.", chatId);
                    return;
                }


                switch (NavigationDictionary[chatId])
                {
                    case NavigationEnum.ReminderMenu:
                        await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                        break;

                    case NavigationEnum.AddReminder:
                        outputTxt = ReminderCommands.Нагадування.ToString();
                        isOk = true;
                        break;
                    
                    case NavigationEnum.StartPeriodMenu:
                        await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                        break;

                    case NavigationEnum.PeriodMenu:
                        outputTxt = PeriodCommands.Період.ToString();
                        isOk = true;
                        break;

                    case NavigationEnum.addPeriod:
                        outputTxt = PeriodCommands.Перiод.ToString();
                        isOk = true;
                        break;

                    case NavigationEnum.addNotifyTheUser:
                        await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                        objectControllerBase.SetDictionary(chatId, NavigationEnum.PeriodMenu, NavigationDictionary);
                        break;

                    default:
                        await PrintMessage("Щось не так.\nНатискай /start.", chatId);
                        break;
                }
            });

            task.Wait();

            outputText = outputTxt;
            return isOk;
        }
  
        /// <summary>
        /// Receives the callback and forwards it.
        /// </summary>
        /// <param name="callbackQuery">Callback</param>
        public async void CallbackQueryAsync(CallbackQuery callbackQuery, Update update, CancellationToken token)
        {
            long chatId = callbackQuery.Message.Chat.Id;
            
            if (callbackQuery.Data.Contains(CallbackQueryCommands.saveReminder.ToString()))
            {
                if(reminderController.Save(chatId, out int f))
                    await PrintMessage("Нагадування збережено.", chatId);
                else
                    await PrintMessage("Нагадування не збережено.", chatId);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteReminder.ToString()))
            {
                objectControllerBase.Delete<Reminder>(callbackQuery, new DataBaseContextForReminder(), "Нагадування");
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.savePeriod.ToString()))
            {
                if(periodController.Save(chatId, out int f))
                    await PrintMessage($"Налаштування успішно збережено.", chatId);
                else
                    await PrintMessage("Налаштування не збережено.", chatId);

            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deletePeriod.ToString()))
            {
                objectControllerBase.Delete<Period>(callbackQuery, new DataBaseContextForPeriod(), "Цикл та користувачі");
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.sendInviteNotifyTheUser.ToString()))
            {
                await notifyTheUserController.SendInvite(chatId, update, CallbackQueryCommands.confirmInvite.ToString());
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.confirmInvite.ToString()))
            {
                if(notifyTheUserController.Save(chatId, out int f))
                    await PrintMessage("Користувач успішно добавлений.", chatId);
                else
                    await PrintMessage("Користувача не добавлено.", chatId);

            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteNotifyTheUser.ToString()))
            {
                objectControllerBase.Delete<NotifyTheUser>(callbackQuery, new DataBaseContextForPeriod(), "Користувач");
            }
        }
        
        #region AddObject

        protected async Task<bool> AddObject(long chatId, string messageText, string replaceText, string buttonName, Func<long, CancellationToken, Update, Task<bool>> deleagate, CancellationToken cancellationToken, Update update)
        {
            bool isOk = false;
            if (IsStartAddedObjectDictionary[chatId])
            {
                objectControllerBase.SetDictionary(chatId, AddReminderEnum.addReminder, AddReminderDictionary);
                objectControllerBase.SetDictionary(chatId, AddPeriodEnum.addPeriod, AddPeriodDictionary);
                objectControllerBase.SetDictionary(chatId, AddNotifyTheUserEnum.addNotifyTheUser, AddNotifyTheUserDictionary);
                IsStartAddedObjectDictionary[chatId] = false;
            }

            if (InputTextDictionary.ContainsKey(chatId))
                objectControllerBase.SetDictionary(chatId, messageText.Replace(replaceText, ""), InputTextDictionary);

            if (AddReminderDictionary.ContainsKey(chatId)
            && InputTextDictionary.ContainsKey(chatId))
            {
                if (await deleagate.Invoke(chatId, cancellationToken, update))
                {
                    objectControllerBase.SetDictionary(chatId, "", CurrentOperationDictionary);
                    objectControllerBase.SetDictionary(chatId, true, IsStartAddedObjectDictionary);
                    isOk = true;
                }
                else
                {
                    objectControllerBase.SetDictionary(chatId, buttonName, CurrentOperationDictionary);
                    objectControllerBase.SetDictionary(chatId, "", InputTextDictionary);
                }
            }
            return isOk;
        }

        protected async Task<bool> AddReminder(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (reminderController.Add(InputTextDictionary[chatId], AddReminderDictionary[chatId], chatId, cancellationToken, out AddReminderDictionary, out NavigationDictionary))
                return true;
            else
                return false;
        }

        protected async Task<bool> AddPeriod(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (periodController.Add(InputTextDictionary[chatId], AddPeriodDictionary[chatId], chatId, cancellationToken, out AddPeriodDictionary, out NavigationDictionary, out bool isExistPeriodOut))
            {
                if (isExistPeriodOut)
                {
                    objectControllerBase.SetDictionary(chatId, "", CurrentOperationDictionary);
                    return true;
                }
                else
                {
                    await periodController.DisplayCurrentPeriod(chatId, CallbackQueryCommands.savePeriod.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                    objectControllerBase.SetDictionary(chatId, "", InputTextDictionary);
                    return true;
                }
            }
            return false;
        }

        protected async Task<bool> AddNotifyTheUser(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (notifyTheUserController.Add(InputTextDictionary[chatId], AddNotifyTheUserDictionary[chatId], update, chatId, cancellationToken,out AddNotifyTheUserDictionary,out NavigationDictionary)) 
                return true;
            else
                return false;
        }

        #endregion

        public void ClearDictionary()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    reminderController.ClearDictionary();
                    periodController.ClearDictionary();
                    notifyTheUserController.ClearDictionary();
                    Task.Delay(60000).Wait();
                }
            });
        }
    }
}

