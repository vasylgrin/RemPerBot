using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.Controller;
using RemBerBot_BL.Controller.DataBase;
using RemBerBot_BL.Controller.Interface;
using RemBerBot_BL.Models;
using Telegram.Bot.Types;
using static RemBerBot_BL.Controller.Controller.ObjectControllerBase;

namespace MySuperUniversalBot_BL.Controller
{
    public class BotController : BotControllerBase
    {
        #region Variable

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
        public async void CheckAnswerAsync(string messageText, long chatId, Update update, CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                InputTextDictionary.SetStartValueOfDictionaryForNewUser(chatId);
                CurrentOperationDictionary.SetStartValueOfDictionaryForNewUser(chatId);
                IsStartAddedObjectDictionary.SetStartValueOfDictionaryForNewUser(chatId);

                if (!DictionaryController.OperationDictionary.ContainsKey(chatId))
                {
                    DictionaryController.OperationDictionary.Add(chatId, OperationEnum.empty);
                }
                if (!DictionaryController.NavigationDictionary.ContainsKey(chatId))
                {
                    DictionaryController.NavigationDictionary.Add(chatId, NavigationEnum.MainMenu);
                }

                #region validations answer


                if (messageText == GeneralCommands.Назад.ToString())
                {
                    bool isOk = await Task.Run(() =>
                    {
                        return BackButton(chatId, out messageText, cancellationToken);
                    });

                    if (isOk)
                    {
                        return;
                    }
                }
                if (messageText == "/start")
                {
                    PrintInline("Привіт, мене звати RemPer.", chatId, SetupInLine("Пройти туторіал", CallbackQueryCommands.tutorialReminder.ToString()));
                    PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(GeneralCommands.Нагадування.ToString(), GeneralCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                }
                else if (messageText == GeneralCommands.Нагадування.ToString())
                {
                    PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Додати.ToString(), GeneralCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.MainMenu);
                    DictionaryController.OperationDictionary.SetDictionary(chatId, OperationEnum.empty);
                }
                else if (messageText == GeneralCommands.Додати.ToString() || GeneralCommands.Додати.ToString() == CurrentOperationDictionary[chatId])
                {
                    SetOperationDictionary(chatId, OperationEnum.addBirthDate);

                    if (await AddObject(messageText, GeneralCommands.Додати.ToString(), chatId, cancellationToken, reminderController))
                    {
                        reminderController.PrintCurrentObject(chatId, CallbackQueryCommands.saveReminder.ToString());
                        PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Додати.ToString(), GeneralCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                        InputTextDictionary.SetDictionary(chatId, "");
                    }
                }
                else if (messageText == GeneralCommands.Переглянути.ToString())
                {
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.MainMenu);
                    objectControllerBase.Display<BirthDate>(chatId, "У тебе немає нагадувань, час їх створити.", CallbackQueryCommands.deleteReminder);
                }
                else if (messageText == GeneralCommands.Період.ToString())
                {
                    PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Перiод.ToString(), GeneralCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.MainMenu);
                }
                else if (messageText == GeneralCommands.Перiод.ToString())
                {
                    PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Створити.ToString(), GeneralCommands.Проглянути.ToString(), GeneralCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);

                    DictionaryController.OperationDictionary.SetDictionary(chatId, OperationEnum.empty);
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.PeriodMenu);
                }
                else if (messageText == GeneralCommands.Створити.ToString() || GeneralCommands.Створити.ToString() == CurrentOperationDictionary[chatId])
                {
                    SetOperationDictionary(chatId, OperationEnum.addPeriod);

                    if (!periodController.isExistPeriod(chatId, out int periodId))
                    {
                        if (await AddObject(messageText, GeneralCommands.Створити.ToString(), chatId, cancellationToken, periodController))
                        {
                            periodController.PrintCurrentObject(chatId, CallbackQueryCommands.savePeriod.ToString());
                            PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Створити.ToString(), GeneralCommands.Проглянути.ToString(), GeneralCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                            InputTextDictionary.SetDictionary(chatId, "");
                        }
                    }
                    else
                    {
                        CurrentOperationDictionary[chatId] = "";
                        PrintInline("Цикл вже створений, якщо хочеш щось змінити то просто видали його.", chatId, SetupInLine(CallbackQueryCommands.Видалити.ToString(), $"{CallbackQueryCommands.deletePeriod} {periodId}"));
                    }
                }
                else if (messageText == GeneralCommands.Проглянути.ToString())
                {
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.PeriodMenu);
                    objectControllerBase.Display<Period>(chatId, "У тебе немає створеного циклу, вже самий час його створити.", CallbackQueryCommands.deletePeriod);
                }
                else if (messageText == GeneralCommands.Розпочались.ToString())
                {
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.PeriodMenu);
                    new PeriodController().Update(chatId);
                }
                else if (messageText == GeneralCommands.Користувачі.ToString())
                {
                    PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Добавити.ToString(), GeneralCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);

                    DictionaryController.OperationDictionary.SetDictionary(chatId, OperationEnum.empty);
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.PeriodMenu);
                }
                else if (messageText == GeneralCommands.Добавити.ToString() || GeneralCommands.Добавити.ToString() == CurrentOperationDictionary[chatId])
                {
                    SetOperationDictionary(chatId, OperationEnum.addNotifyTheUser);

                    if (periodController.isExistPeriod(chatId, out int perId))
                    {
                        if (await AddObject(messageText, GeneralCommands.Добавити.ToString(), chatId, cancellationToken, notifyTheUserController))
                        {
                            notifyTheUserController.PrintCurrentObject(chatId, CallbackQueryCommands.sendInviteNotifyTheUser.ToString());
                            PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Добавити.ToString(), GeneralCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), cancellationToken);
                            InputTextDictionary.SetDictionary(chatId, "");
                        }
                    }
                    else
                    {
                        PrintMessage("Твій цикл не знайдено, саме час його створити.", chatId);
                        CurrentOperationDictionary[chatId] = "";
                    }
                }
                else if (messageText == GeneralCommands.Продивитись.ToString())
                {
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.addNotifyTheUser);
                    objectControllerBase.Display<NotifyTheUser>(chatId, "У тебе немає доданих користувачів, здається, самий час їх добавити.", CallbackQueryCommands.deleteNotifyTheUser);
                }
                else if (messageText == GeneralCommands.myId.ToString())
                {
                    DictionaryController.NavigationDictionary.SetDictionary(chatId, NavigationEnum.addNotifyTheUser);
                    PrintMessage($"Id твого чату:", chatId);
                    PrintMessage($"{chatId}", chatId);
                }
                else
                {
                    PrintMessage("Невідома команда...", chatId);
                }

                #endregion

            });
        }

        private bool BackButton(long chatId, out string outputText, CancellationToken cancellationToken)
        {
            bool isOk = false;
            outputText = "";
            CurrentOperationDictionary[chatId] = "";
            DictionaryController.OperationDictionary[chatId] = OperationEnum.empty;

            switch (DictionaryController.NavigationDictionary[chatId])
            {
                case NavigationEnum.MainMenu:
                    PrintKeyboard("Обирай дію:", chatId, SetupKeyboard(GeneralCommands.Нагадування.ToString(), GeneralCommands.Період.ToString(), GeneralCommands.myId.ToString()), cancellationToken);
                    isOk = true;
                    break;

                case NavigationEnum.addBirthDate:
                    outputText = GeneralCommands.Нагадування.ToString();
                    break;

                case NavigationEnum.PeriodMenu:
                    outputText = GeneralCommands.Період.ToString();
                    break;

                case NavigationEnum.addPeriod:
                    outputText = GeneralCommands.Перiод.ToString();
                    break;

                case NavigationEnum.addNotifyTheUser:
                    outputText = GeneralCommands.Користувачі.ToString();
                    break;
            }

            return isOk;
        }

        private void SetOperationDictionary(long chatId, OperationEnum operationEnum)
        {
            if (DictionaryController.OperationDictionary.ContainsKey(chatId))
            {
                if (DictionaryController.OperationDictionary[chatId] == OperationEnum.empty)
                {
                    DictionaryController.OperationDictionary[chatId] = operationEnum;
                }
            }
            else if (!DictionaryController.OperationDictionary.ContainsKey(chatId))
            {
                DictionaryController.OperationDictionary.Add(chatId, operationEnum);
            }
        }

        /// <summary>
        /// Receives the callback and forwards it.
        /// </summary>
        /// <param name="callbackQuery">Callback</param>
        public void CallbackQueryAsync(CallbackQuery callbackQuery, Update update, CancellationToken token)
        {
            long chatId = callbackQuery.Message!.Chat.Id;

            if (callbackQuery.Data!.Contains(CallbackQueryCommands.saveReminder.ToString()))
            {
                if (reminderController.Save(chatId, out int f))
                    PrintMessage("Нагадування збережено.", chatId);
                else
                    PrintMessage("Нагадування не збережено.", chatId);
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteReminder.ToString()))
            {
                objectControllerBase.Delete<BirthDate>(callbackQuery, new RemPerContext(), "Нагадування");
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.savePeriod.ToString()))
            {
                if (periodController.Save(chatId, out int f))
                    PrintMessage($"Налаштування успішно збережено.", chatId);
                else
                    PrintMessage("Налаштування не збережено.", chatId);

            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deletePeriod.ToString()))
            {
                objectControllerBase.Delete<Period>(callbackQuery, new RemPerContext(), "Цикл та користувачі");
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.sendInviteNotifyTheUser.ToString()))
            {
                notifyTheUserController.SendInvite(chatId, update, CallbackQueryCommands.confirmInvite.ToString());
            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.confirmInvite.ToString()))
            {
                if (notifyTheUserController.Save(chatId, out int f))
                    PrintMessage("Користувач успішно добавлений.", chatId);
                else
                    PrintMessage("Користувача не добавлено.", chatId);

            }
            else if (callbackQuery.Data.Contains(CallbackQueryCommands.deleteNotifyTheUser.ToString()))
            {
                objectControllerBase.Delete<NotifyTheUser>(callbackQuery, new RemPerContext(), "Користувач");
            }
        }

        private async Task<bool> AddObject(string messageText, string buttonName, long chatId, CancellationToken cancellationToken, IObjectControllerBase iObjectControllerBase)
        {
            bool isOk = false;

            if (IsStartAddedObjectDictionary[chatId])
            {
                if (await iObjectControllerBase.Add(messageText, chatId, cancellationToken))
                {
                    InputTextDictionary.SetDictionary(chatId, "");
                    CurrentOperationDictionary.SetDictionary(chatId, "");
                    isOk = true;
                }
                else
                {
                    CurrentOperationDictionary.SetDictionary(chatId, buttonName);
                }
            }

            return isOk;
        }
    }
}
