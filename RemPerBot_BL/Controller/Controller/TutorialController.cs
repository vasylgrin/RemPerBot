using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.DataBase;
using RemBerBot_BL.Controller.Interface;
using RemBerBot_BL.Models;
using Telegram.Bot.Types;

namespace RemBerBot_BL.Controller.Controller
{
    public class TutorialController : BotControllerBase
    {
        #region Variable

        Dictionary<long, string> InputTextDictionary = new();
        Dictionary<long, string> CurrentOperationDictionary = new();
        Dictionary<long, bool> IsStartAddedObjectDictionary = new();

        ReminderController reminderController = new();
        PeriodController periodController = new();
        NotifyTheUserController notifyTheUserController = new();
        ObjectControllerBase objectControllerBase = new();

        Dictionary<long, NotAllowed> TutorialDictionary = new();

        Dictionary<long, string> TutorialUserDictionary = new();

        Dictionary<long, int> IdObjectForClearDataUserTutorial = new();

        #endregion

        #region Enum

        enum NotAllowed
        {
            AddReminder = 1,
            tutorialReminderSave,
            DisplayReminder,
            AddPeriod,
            DisplayPeriod,
            startMenstruation,
            ClickMyIdNotifyTheUser,
            addNotifyTheUser,
            DisplayNotifyTheUser
        }

        enum TutorialReminderEnum
        {
            tutorialReminder,
            addReminder,
            tutorialReminderSave
        }

        enum TutorialPeriodEnum
        {
            tutorialPeriod,
            addPeriod,
            tutorialPeriodSave
        }

        enum TutorialNotifyTheUserEnum
        {
            tutorialNotifyTheUser,
            addNotifyTheUser,
            tutorialNotifyTheUserSave,
            tutorialNotifyTheUserInviteSend,
            tutorialNotifyTheUserConfirmInvite
        }

        #endregion


        #region Tutorial

        public bool Tutorial(long chatId, string inputText, string tutorial, Update update, CancellationToken token)
        {
            bool isOk = false;

            InputTextDictionary.SetStartValueOfDictionaryForNewUser(chatId);
            CurrentOperationDictionary.SetStartValueOfDictionaryForNewUser(chatId);
            IsStartAddedObjectDictionary.SetStartValueOfDictionaryForNewUser(chatId);

            if (inputText.Contains(GeneralCommands.Назад.ToString()))
            {
                PrintInline("Ти впевнений(а) що хочеш завершити туторчик?", chatId, SetupInLine("Так", "tutorialComplete"));
            }
            else if (inputText.Contains("tutorialComplete"))
            {
                PrintMessage("Туторчик завершений.", chatId);
                ClearEndDataTutorial(chatId);
                isOk = true;
            }
            else if (tutorial.Contains(TutorialReminderEnum.tutorialReminder.ToString()))
            {
                TutorialReminder(chatId, inputText, update, token);
            }
            else if (tutorial.Contains(TutorialPeriodEnum.tutorialPeriod.ToString()))
            {
                TutorialPeriod(chatId, inputText, update, token);
            }
            else if (tutorial.Contains(TutorialNotifyTheUserEnum.tutorialNotifyTheUser.ToString()))
            {
                TutorialNotifyTheUser(chatId, inputText, update, token);
            }

            return isOk;
        }

        private async void TutorialReminder(long chatId, string inputText, Update update, CancellationToken token)
        {
            if (inputText == TutorialReminderEnum.tutorialReminder.ToString())
            {
                PrintKeyboard("Я твій помічник у двох запиатаннях:\nНагадування та Період овуляції.\nПочнемо з нагадувань, тому\nНатисни Нагадування.", chatId, SetupKeyboard(GeneralCommands.Нагадування.ToString(), GeneralCommands.Період.ToString(), GeneralCommands.myId.ToString()), token);
                PrintMessage("Щоб завершити навчання тисни Назад, в даному випадку потрібно зайти в Нагадування і там буде кнопка Назад.", chatId, 0);
            }
            else if (inputText == GeneralCommands.Нагадування.ToString())
            {
                PrintKeyboard("Розділ нагадування створений для того щоб ти міг(могла) додавати Дні народження." ,chatId, SetupKeyboard(GeneralCommands.Додати.ToString(), GeneralCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                PrintMessage($"Перед тобою з'явились три кнопки: \nДодати, Переглянути та Назад." +
                $"\nМожливо ти вже дагадався(лась) що ці кнопки виконують, але ми зараз розглянемо детальніше." +
                $"\nНатисни Додати щоб додати нове нагадування.", chatId);
                
                TutorialDictionary.SetDictionary(chatId, NotAllowed.AddReminder);
            }
            else if (inputText == GeneralCommands.Додати.ToString() || GeneralCommands.Додати.ToString() == CurrentOperationDictionary[chatId])
            {
                if (TutorialDictionary[chatId] == NotAllowed.AddReminder)
                {
                    if (await AddObject(inputText, GeneralCommands.Додати.ToString(), chatId, token, reminderController))
                    {
                        PrintKeyboard("Твоє нагадування поки знаходитсья в моїй цифровій макітрі, щоб я його запам'ятав перевірь дані та натискай Зберегти.", chatId, SetupKeyboard(GeneralCommands.Додати.ToString(), GeneralCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                        reminderController.PrintCurrentObject(chatId, TutorialReminderEnum.tutorialReminderSave.ToString());
                        TutorialDictionary.SetDictionary(chatId, NotAllowed.tutorialReminderSave);
                    }
                }
                else
                {
                    PrintMessage("Притримуйся інструкції будь ласка.", chatId);
                    return;
                }
            }
            else if (inputText == TutorialReminderEnum.tutorialReminderSave.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.tutorialReminderSave)
                {
                    if (reminderController.Save(chatId, out int reminderId))
                    {
                        PrintMessage("Таакссс... Ну я його постараюсь не забути, а ти тепер ти можеш перевірити активні нагадування, " + "та видалити їх якщо вони тобі не потрібні. Щоб зробити це" + "\nНатисни Переглянути.", chatId, 2000);
                        TutorialDictionary.SetDictionary(chatId, NotAllowed.DisplayReminder);

                        IdObjectForClearDataUserTutorial.Add(chatId, reminderId);
                    }
                }
            }
            else if (inputText == GeneralCommands.Переглянути.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.DisplayReminder)
                {
                    objectControllerBase.Display<BirthDate>(chatId, "У тебе немає нагадувань, час їх створити.", CallbackQueryCommands.deleteReminder);
                    Task.Delay(1000).Wait();
                    PrintInline("Ну що ж тобі сказати друже, постараюсь не забути, " + "а ти поки можеш тут нууу... як тобі сказати, дивитись мої можливості далі. Так що\nТисни Далі", chatId, SetupInLine("Далі", TutorialPeriodEnum.tutorialPeriod.ToString()));
                    return;
                }

                if (TutorialDictionary[chatId] == NotAllowed.AddReminder)
                {
                    PrintMessage("Притримуйся інструкції будь ласка, тисни Додати.", chatId);
                    return;
                }
                else if (TutorialDictionary[chatId] == NotAllowed.tutorialReminderSave)
                {
                    PrintMessage("Притримуйся інструкції будь ласка, тисни Зберегти.", chatId);
                    return;
                }

            }
            else if (inputText == GeneralCommands.Період.ToString())
            {
                PrintMessage("Ой... Ти переплутав(ла) кнопку, тисни лівіше.", chatId);
            }
            else if (inputText == GeneralCommands.myId.ToString())
            {
                PrintMessage("Поки що тобі це не потрібно, тому тисни Нагадування.", chatId);
            }
            else
            {
                PrintMessage("Щось невідоме... Перевірь та спробуй ще раз.", chatId);
            }
        }

        private async Task TutorialPeriod(long chatId, string inputText, Update update, CancellationToken token)
        {
            if (inputText == TutorialPeriodEnum.tutorialPeriod.ToString())
            {
                PrintKeyboard("Далі у нас по списку це розділ \"Період овуляції\" скорочено Період." + "\nЦей розділ створений для відстеження періоду овуляції твоєї двічини, друга і т.д., тобто він працює як звичайний календарик який є у майже кожної дівчини, але я не займаю жодного місця на твоєму смартфоні, про це турбується мій розробник, тому +1 в мою сторону, а ми йдемо далі так що швидше натискай Період і погнали.", chatId, SetupKeyboard(GeneralCommands.Нагадування.ToString(), GeneralCommands.Період.ToString(), GeneralCommands.myId.ToString()), token);
                return;
            }
            else if (inputText == GeneralCommands.Нагадування.ToString())
            {
                PrintMessage("Тобі потрібно трішечки правіше.", chatId);
                return;
            }
            else if (inputText == GeneralCommands.Період.ToString())
            {
                PrintKeyboard("Перед тобою з'явились три кнопки:\nПеріод, Користувачі та Назад.\nРозділ Користувачі ми роглянемо трішечки пізніше, тому лясни ти вже цю кнопку Період.", chatId, SetupKeyboard(GeneralCommands.Перiод.ToString(), GeneralCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), token);
                return;
            }
            else if (inputText == GeneralCommands.Перiод.ToString())
            {
                PrintKeyboard("Супер, тепер перед тобою з'явились чотири кнопки: Створити, Проглянути, Розпочались та Назад." +
                    "\nНатиснувши Створити ти зможеш добавити свій цикл Овуляції, ввівши тривалість менструації, циклу, дати останьої менструації," +
                    " але навіщо я тобі це розповідаю якщо ти можеш сам(а) перевірити, хутчіш \nНатискай Створити.",
                    chatId, SetupKeyboard(GeneralCommands.Створити.ToString(), GeneralCommands.Проглянути.ToString(), GeneralCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), token);

                TutorialDictionary.SetDictionary(chatId, NotAllowed.AddPeriod);
                return;
            }
            else if (inputText == GeneralCommands.Створити.ToString() || GeneralCommands.Створити.ToString() == CurrentOperationDictionary[chatId])
            {
                if (TutorialDictionary[chatId] == NotAllowed.AddPeriod)
                {
                    if (await AddObject(inputText, GeneralCommands.Створити.ToString(), chatId, token, periodController))
                    {
                        periodController.PrintCurrentObject(chatId, CallbackQueryCommands.savePeriod.ToString());
                        PrintKeyboard("Обирай:", chatId, SetupKeyboard(GeneralCommands.Створити.ToString(), GeneralCommands.Проглянути.ToString(), GeneralCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), token);
                        TutorialDictionary.SetDictionary(chatId, NotAllowed.DisplayPeriod);
                    }
                    return;
                }

                PrintMessage("Стривай... Не зараз.", chatId);
                return;
            }
            else if (inputText == TutorialPeriodEnum.tutorialPeriodSave.ToString())
            {
                if (periodController.Save(chatId, out int periodId))
                {
                    PrintMessage("Ти ж моя розумашка, тепер ти можеш проглянути свій цикл.\nТисни Проглянути.", chatId, 2000);
                    TutorialDictionary.SetDictionary(chatId, NotAllowed.DisplayPeriod);

                    IdObjectForClearDataUserTutorial.Add(chatId, periodId);
                }
            }
            else if (inputText == GeneralCommands.Проглянути.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.DisplayPeriod)
                {
                    PrintMessage("Тут ти можеш перевірити свій цикл та видалити його якщо захочеш щось змінити, все наче просто, тому йдемо далі.", chatId, 2000);
                    objectControllerBase.Display<Period>(chatId, "У тебе немає створеного циклу, вже самий час його створити.", CallbackQueryCommands.deletePeriod);
                    PrintMessage("Кнопка Розпочались створена для того щоб розпочати відлік днів менструації, так як інколи бувають затримки або можуть раніше розпочатись \"ці дні\"." +
                        "\nДля перевірки та розуміння її роботи, натискай Розпочались(якщо ти ввів(ввела) все як було вказано вище то все буде добре, якщо ні то може бути сюрприз у вигляді помилки).", chatId, 2000);
                    TutorialDictionary.SetDictionary(chatId, NotAllowed.startMenstruation);
                    return;
                }

                PrintMessage("Не попав... Спробуй трішечки лівіше.", chatId);
                return;
            }
            else if (inputText == GeneralCommands.Розпочались.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.startMenstruation)
                {
                    new PeriodController().Update(chatId);
                    PrintInline("Ну ось ти й розпочав(ла) свій перший цикл, наче все просто тому йдемо далі.\nНатисни Далі.", chatId, SetupInLine("Далі", TutorialNotifyTheUserEnum.tutorialNotifyTheUser.ToString()));
                    return;

                }

                PrintMessage("Ні, не це тобі потрібно.", chatId);
                return;
            }
            else if (inputText == GeneralCommands.Користувачі.ToString())
            {
                PrintMessage("Йой... Ти щось промазав, тобі потрібен Період.", chatId);
                return;
            }
            else if (inputText == GeneralCommands.myId.ToString())
            {
                PrintMessage("Не туди... Хмм... Цікава фраза.", chatId);
                return;
            }
            else
            {
                PrintMessage("Щось невідоме... Перевірь та спробуй ще раз.", chatId);
            }
        }

        private async Task TutorialNotifyTheUser(long chatId, string inputText, Update update, CancellationToken token)
        {
            if (inputText == TutorialNotifyTheUserEnum.tutorialNotifyTheUser.ToString())
            {
                PrintKeyboard("І саме головне, це моя фішка яку я можу тобі запропонувати(окрім того що я не займаю жодного місця на твоєму пристрої) " +
                "це сповішувати про твій цикл користувачів, яких ти добавиш вівши їх ідентифікатор чату та ім'я яке будеш бачити тільки ти, " +
                "тому ти можеш ввести будь яке ім'я і ніхто про це не дізнається." +
                "\nДля того щоб твої корситувачі отримували повідомлення вони повинні зробити декілька простих кроків, це запустити бота(натиснути start), " +
                "та відправити тобі свій Chat Id який можна дізнатись на початку додатку та у цьому розділі." +
                "\nТому тисни Користувачі і продовжимо твоє навчання.", chatId, SetupKeyboard(GeneralCommands.Перiод.ToString(), GeneralCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), token);

                return;
            }
            else if (inputText == GeneralCommands.Перiод.ToString())
            {
                PrintMessage("Друже, натисни правіше...", chatId);
                return;
            }
            else if (inputText == GeneralCommands.Користувачі.ToString())
            {
                TutorialDictionary.SetDictionary(chatId, NotAllowed.ClickMyIdNotifyTheUser);
                PrintKeyboard("Перед тобою чотири кнопки:\nДобавити, Продивитись, MyId та Назад." + "Все впринципі працює та виглядає як те, що ти бавчи раніше, тому я думаю у тебе не виникнуть проблеми з цим, але перед тим як добавити свого першого користувача(в даному випадку ти добавиш сам себе), дізнайся свій Chat Id." + "\nНатисни MyId.", chatId, SetupKeyboard(GeneralCommands.Добавити.ToString(), GeneralCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), token);
                return;
            }
            else if (inputText == GeneralCommands.myId.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.ClickMyIdNotifyTheUser)
                {
                    PrintMessage($"Id твого чату:", chatId);
                    PrintMessage($"{chatId}", chatId);

                    PrintMessage("Тепер натисни Добавити, та дотримуйся інструкції.", chatId);

                    TutorialDictionary.SetDictionary(chatId, NotAllowed.addNotifyTheUser);
                    return;
                }

                PrintMessage("Не зараз... Дотримуйя інструкції.", chatId);
                return;
            }
            else if (inputText == GeneralCommands.Добавити.ToString() || GeneralCommands.Добавити.ToString() == CurrentOperationDictionary[chatId])
            {
                if (TutorialDictionary[chatId] == NotAllowed.addNotifyTheUser)
                {
                    if (await AddObject(inputText, GeneralCommands.Добавити.ToString(), chatId, token, notifyTheUserController))
                    {
                        PrintMessage("Принцип точно такий самий, перевіряєш та відправляєш запрошення користувачу, і після того як він його прийме, йому будуть приходити новини про твій цикл.\nНатискай Запросити.", chatId);
                        notifyTheUserController.PrintCurrentObject(chatId, TutorialNotifyTheUserEnum.tutorialNotifyTheUserInviteSend.ToString());
                    }
                    return;
                }

                PrintMessage("Не та кнопка, читай уважніше вказання.", chatId);
                return;
            }
            else if (inputText == TutorialNotifyTheUserEnum.tutorialNotifyTheUserInviteSend.ToString())
            {
                PrintMessage("Ось так виглядає твоє запрошення коли ти його відправляєш користувачу. Щоб прийняти запрошення \nНатисни Підтвердити.", chatId);
                notifyTheUserController.SendInvite(chatId, update, TutorialNotifyTheUserEnum.tutorialNotifyTheUserConfirmInvite.ToString());
            }
            else if (inputText == TutorialNotifyTheUserEnum.tutorialNotifyTheUserConfirmInvite.ToString())
            {
                if (notifyTheUserController.Save(chatId, out int notifyTheUserId))
                {
                    PrintMessage("Вітаю... Тепер у тебе є перший добавлений користувач, далі що тобі потрібно зробити це продивитись своїх користувачів. \nНатисни Продивитись.", chatId);
                    TutorialDictionary.SetDictionary(chatId, NotAllowed.DisplayNotifyTheUser);

                    IdObjectForClearDataUserTutorial.Add(chatId, notifyTheUserId);
                }
            }
            else if (inputText == GeneralCommands.Продивитись.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.DisplayNotifyTheUser)
                {
                    objectControllerBase.Display<Period>(chatId, "У тебе немає доданих користувачів, мені здається самий час їх добавити.", CallbackQueryCommands.deleteNotifyTheUser);
                    Task.Delay(2000).Wait();

                    PrintMessage("Ехх... Ти так швидко все запамятовуєш що я навіть не встиг додати 2 + 2." +
                        "\nНа цьому туторчик закінчився, тому я та мій розробник бажаємо тобі не забувати нічого що б не створювати Нагадування, та безболісного циклу.", chatId, 2000);

                    new DataBaseControllerBase<Period>(new RemPerContext()).Load().Where(x => x.ChatId == chatId).ToList().ForEach(x =>
                      {
                          new DataBaseControllerBase<Period>(new RemPerContext()).Remove(x);
                      });
                    return;
                }

                PrintMessage("Будь ласка дотримуйся інструкції.", chatId);
                return;
            }
            else
            {
                PrintMessage("Невідома команда...", chatId);
            }

        }

        #endregion

        public void ClearEndDataTutorial(long chatId)
        {
            Clear<BirthDate>(new RemPerContext(), chatId);
            Clear<Period>(new RemPerContext(), chatId);
            Clear<NotifyTheUser>(new RemPerContext(), chatId);
        }

        private void Clear<G>(DbContext context, long chatId) where G : ModelBase
        {
            if (IdObjectForClearDataUserTutorial.ContainsKey(chatId))
            {
                DataBaseControllerBase<G> dataBaseControllerBase = new(context);

                dataBaseControllerBase.Load().Where(x => x.Id == IdObjectForClearDataUserTutorial[chatId]).ToList().ForEach(x =>
                {
                    dataBaseControllerBase.Remove(x);
                });
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



        public async Task<bool> CheckTutorial(long chatId, string messageText, string tutorialUser, Update update, CancellationToken cancellationToken)
        {
            bool isOk = false;

            TutorialUserDictionary.SetDictionary(chatId, tutorialUser);

            if (TutorialUserDictionary.ContainsKey(chatId))
            {
                if (Tutorial(chatId, messageText, TutorialUserDictionary[chatId], update, cancellationToken))
                    isOk = true;
            }
            else
            {
                TutorialUserDictionary.Add(chatId, messageText);
                await CheckTutorial(chatId, messageText, tutorialUser, update, cancellationToken);
            }
            return isOk;
        }
    }
}
