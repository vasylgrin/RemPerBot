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

        #endregion

        /// <summary>
        /// Receives text and forwards it.
        /// </summary>
        /// <param name="messageText">Data.</param>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async Task CheckAnswerAsync(string messageText, long chatId, Update update, CancellationToken cancellationToken)
        {
            vBotControllerBase(chatId, cancellationToken);

            SetsTheInitialValue(chatId,InputTextDictionary, CurrentOperationDictionary, IsStartAddedObjectDictionary);

        #region validations answer

        start:           
            if (messageText == "/start")
            {
                //SetDictionary<int>(chatId, NavigationEnum.ReminderMenu, NavigationDictionary);
                await PrintInline("Привіт, мене звати RemPer.", chatId, SetupInLine("Пройти туторіал", CallbackQueryCommands.tutorialNotifyTheUser.ToString()));
                await PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
            }
            else if (messageText == GeneralCommands.Назад.ToString())
            {
                if(BackButton(chatId, out messageText, cancellationToken))
                {
                    goto start;
                }
            }
            else if (messageText == ReminderCommands.Нагадування.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary(chatId, NavigationEnum.ReminderMenu, NavigationDictionary);
            }
            else if (messageText == ReminderCommands.Додати.ToString() || ReminderCommands.Додати.ToString() == CurrentOperationDictionary[chatId])
            {
                if (AddObject(chatId, messageText, ReminderCommands.Додати.ToString(), ReminderCommands.Додати.ToString(), AddReminder, cancellationToken, update))
                {
                    await reminderController.DisplayCurrentReminder(chatId, CallbackQueryCommands.saveReminder.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                }
            }
            else if (messageText == ReminderCommands.Переглянути.ToString())
            {
                await new ReminderController().DisplayReminder(chatId, cancellationToken);
            }
            else if (messageText == PeriodCommands.Період.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Перiод.ToString(), NotifyTheUserCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary(chatId, NavigationEnum.StartPeriodMenu, NavigationDictionary);
            }
            else if (messageText == PeriodCommands.Перiод.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary(chatId, AddPeriodEnum.empty, AddPeriodDictionary);
                SetDictionary(chatId, NavigationEnum.PeriodMenu, NavigationDictionary);
            }
            else if (messageText == PeriodCommands.Створити.ToString() || PeriodCommands.Створити.ToString() == CurrentOperationDictionary[chatId])
            {
                if(AddObject(chatId, messageText, PeriodCommands.Створити.ToString(), PeriodCommands.Створити.ToString(), AddPeriod, cancellationToken, update))
                {
                    await periodController.DisplayCurrentPeriod(chatId, CallbackQueryCommands.savePeriod.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                }
            }
            else if (messageText == PeriodCommands.Проглянути.ToString())
            {
                SetDictionary(chatId, AddPeriodEnum.empty, AddPeriodDictionary);
                new PeriodController().DisplayPeriod(chatId, cancellationToken);
            }
            else if (messageText == PeriodCommands.Розпочались.ToString())
            {
                new PeriodController().UpdatePeriodAsync(chatId);
            }
            else if (messageText == NotifyTheUserCommands.Користувачі.ToString())
            {
                await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                SetDictionary(chatId, NavigationEnum.PeriodMenu, NavigationDictionary);
            }
            else if (messageText == NotifyTheUserCommands.Добавити.ToString() || NotifyTheUserCommands.Добавити.ToString() == CurrentOperationDictionary[chatId])
            {
                if(AddObject(chatId, messageText, NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Добавити.ToString(), AddNotifyTheUser, cancellationToken, update))
                {
                    await notifyTheUserController.DisplayCurrentNotifyTheUser(chatId, CallbackQueryCommands.sendInviteNotifyTheUser.ToString());
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                }
            }
            else if (messageText == NotifyTheUserCommands.Продивитись.ToString())
            {
                await new NotifyTheUserController().DispalyNotifyTheUser(chatId, cancellationToken);
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
            SetDictionary(chatId, AddReminderEnum.addReminder, AddReminderDictionary);
            SetDictionary(chatId, AddPeriodEnum.addPeriod, AddPeriodDictionary);
            SetDictionary(chatId, AddNotifyTheUserEnum.addNotifyTheUser, AddNotifyTheUserDictionary);
            SetDictionary(chatId, "", CurrentOperationDictionary);

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
                        SetDictionary(chatId, NavigationEnum.PeriodMenu, NavigationDictionary);
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
                await reminderController.SaveReminderAsync(chatId);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteReminder.ToString()))
            {
                new ReminderController().DeleteReminder(callbackQuery);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.savePeriod.ToString()))
            {
                await periodController.SavePeriodAsync(chatId);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deletePeriod.ToString()))
            {
                new PeriodController().DeletePeriod(callbackQuery);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.sendInviteNotifyTheUser.ToString()))
            {
                await notifyTheUserController.SendInvite(chatId, update, CallbackQueryCommands.confirmInvite.ToString());
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.confirmInvite.ToString()))
            {
                await notifyTheUserController.SaveNotifyTheUserAsync(chatId);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteNotifyTheUser.ToString()))
            {
                new NotifyTheUserController().DeleteNotifyTheUser(callbackQuery);
            }
        }
        
        #region AddObject

        protected bool AddObject(long chatId, string messageText, string replaceText, string buttonName, Func<long, CancellationToken, Update, bool> deleagate, CancellationToken cancellationToken, Update update)
        {
            bool isOk = false;
            if (IsStartAddedObjectDictionary[chatId])
            {
                SetDictionary(chatId, AddReminderEnum.addReminder, AddReminderDictionary);
                SetDictionary(chatId, AddPeriodEnum.addPeriod, AddPeriodDictionary);
                SetDictionary(chatId, AddNotifyTheUserEnum.addNotifyTheUser, AddNotifyTheUserDictionary);
            }

            if (InputTextDictionary.ContainsKey(chatId))
                SetDictionary(chatId, messageText.Replace(replaceText, ""), InputTextDictionary);

            if (AddReminderDictionary.ContainsKey(chatId)
            && InputTextDictionary.ContainsKey(chatId))
            {
                if (deleagate.Invoke(chatId, cancellationToken, update))
                {
                    SetDictionary(chatId, "", CurrentOperationDictionary);
                    SetDictionary(chatId, true, IsStartAddedObjectDictionary);
                    isOk = true;
                }
                else
                {
                    SetDictionary(chatId, buttonName, CurrentOperationDictionary);
                    SetDictionary(chatId, "", InputTextDictionary);
                }
            }
            return isOk;
        }

        protected bool AddReminder(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (reminderController.AddReminder(InputTextDictionary[chatId], AddReminderDictionary[chatId], chatId, cancellationToken, out AddReminderDictionary, out NavigationDictionary, out InputTextDictionary))
                return true;
            else
                return false;
        }

        protected bool AddPeriod(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (periodController.AddPeriod(InputTextDictionary[chatId], AddPeriodDictionary[chatId], chatId, cancellationToken, out AddPeriodDictionary, out NavigationDictionary, out InputTextDictionary, out bool isExistPeriodOut))
            {
                if (isExistPeriodOut)
                {
                    SetDictionary(chatId, "", CurrentOperationDictionary);
                    return false;
                }
                return true;
            }
            else
                return false;
        }

        protected bool AddNotifyTheUser(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (notifyTheUserController.AddNotifyTheUser(InputTextDictionary[chatId], AddNotifyTheUserDictionary[chatId], update, chatId, cancellationToken,out AddNotifyTheUserDictionary,out NavigationDictionary,out InputTextDictionary)) 
                return true;
            else
                return false;
        }

        #endregion
    }
}

