using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.Controller;
using RemBerBot_BL.Controller.DataBase;
using RemBerBot_BL.Controller.Interface;
using Telegram.Bot.Types;
using static MySuperUniversalBot_BL.Controller.BotControllerBase;

namespace MySuperUniversalBot_BL.Controller
{
    public class NotifyTheUserController : ObjectControllerBase, IObjectControllerBase
    {
        Dictionary<long, long> ChatIdAddedDictionary = new();
        Dictionary<long, string> UsernameAddedDictionary = new();
        Dictionary<long, int> ClearDictionaryUser = new();

        BotControllerBase botControllerBase = new();

        public async Task<bool> Add(string inputText, long chatId, CancellationToken cancellationToken)
        {
            bool isOk = false;

            await Task.Run(() =>
            {
                OperationEnum operationEnum = DictionaryController.OperationDictionary[chatId];
                if (operationEnum == OperationEnum.addNotifyTheUser)
                {
                    botControllerBase.PrintKeyboard("Введи чат Id користувача якого хочеш добавити\nПриклад: 000000001", chatId, botControllerBase.SetupKeyboard(GeneralCommands.Назад.ToString()), cancellationToken);
                    ClearDictionaryUser.SetDictionary(chatId, 5);

                    DictionaryController.NavigationDictionary[chatId] = NavigationEnum.addNotifyTheUser;
                    DictionaryController.OperationDictionary[chatId] = OperationEnum.addNotifyTheUserChatId;
                }
                else if (operationEnum == OperationEnum.addNotifyTheUserChatId)
                {
                    if (long.TryParse(inputText, out long chatIdAdded))
                    {
                        if (inputText.Length >= 6 && inputText.Length <= 10)
                        {
                            if (!isExistUser(chatIdAdded))
                            {
                                ChatIdAddedDictionary.SetDictionary(chatId, chatIdAdded);
                                botControllerBase.PrintMessage("Введи Ім'я користувача. \nНаприклад: Котик\nP.S Це ім'я будеш бачити тільки ти.", chatId);

                                DictionaryController.OperationDictionary[chatId] = OperationEnum.addNotifyTheUserName;
                            }
                            else
                                botControllerBase.PrintMessage("Ви вже добавили цього користувача.", chatId);
                        }
                        else
                            botControllerBase.PrintMessage("Id складається 3 9 цифр, спробуй ще раз.", chatId);
                    }
                    else
                        botControllerBase.PrintMessage("Id має містити лише цифри, спробуй ще раз.", chatId);
                }
                else if (operationEnum == OperationEnum.addNotifyTheUserName)
                {
                    if (string.IsNullOrWhiteSpace(inputText))
                        botControllerBase.PrintMessage("Ім'я не може бути пустим.", chatId);

                    UsernameAddedDictionary.SetDictionary(chatId, inputText);

                    DictionaryController.NavigationDictionary[chatId] = NavigationEnum.PeriodMenu;
                    isOk = true;
                }

            });

            return isOk;
        }

        public bool Save(long chatId, out int objectId)
        {
            objectId = 0;

            if (ChatIdAddedDictionary.ContainsKey(chatId)
                && UsernameAddedDictionary.ContainsKey(chatId))
            {
                var id = new DataBaseControllerBase<Period>(new RemPerContext()).Load().Where(period => period.ChatId == chatId).First().Id;
                new DataBaseControllerBase<NotifyTheUser>(new RemPerContext()).Save(new NotifyTheUser(chatId, ChatIdAddedDictionary[chatId], UsernameAddedDictionary[chatId], id));

                ChatIdAddedDictionary.Remove(chatId);
                UsernameAddedDictionary.Remove(chatId);
                ClearDictionaryUser.Remove(chatId);
                return true;
            }
            return false;
        }

        public void PrintCurrentObject(long chatId, string callbackName)
        {
            botControllerBase.PrintInline($"Дані користувача:\nChat Id:{ChatIdAddedDictionary[chatId]}\nІм'я: {UsernameAddedDictionary[chatId]}", chatId, botControllerBase.SetupInLine(CallbackQueryCommands.Запросити.ToString(), callbackName));
        }

        private bool isExistUser(long chatId)
        {
            bool isOK = false;
            List<Period> Periods = new();

            using (RemPerContext db = new())
            {
                Periods = db.Periods.Include(n => n.NotifyTheUser).ToList();
            }

            Periods.Where(p => p.ChatId == chatId).ToList().ForEach(p =>
            {
                p.NotifyTheUser.Where(n => n.ChatIdAdded == chatId).ToList().ForEach(p =>
                {
                    isOK = true;
                });
            });

            return isOK;
        }

        public void SendInvite(long chatId, Update update, string CallbackTextSendInvite)
        {
            ChatIdAddedDictionary.TryGetValue(chatId, out long temp);
            ChatIdAddedDictionary.TryGetValue(chatId, out long chatIdAdded);
            if (temp != 0)
            {
                botControllerBase.PrintInline($"Запрошення на приймання повідомлень від користувача {update?.CallbackQuery?.Message?.Chat.Username}", chatIdAdded, botControllerBase.SetupInLine(CallbackQueryCommands.Підтвердити.ToString(), CallbackTextSendInvite));
            }
        }
    }
}
