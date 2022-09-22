﻿using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Models;
using Telegram.Bot.Types;

namespace RemBerBot_BL.Controller.Controller
{
    public class TutorialController : BotControllerBase
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

        public async Task<bool> Tutorial(long chatId, string inputText, string tutorial, Update update, CancellationToken token)
        {
            bool isOk = false;

            objectControllerBase.SetsTheInitialValue(chatId, InputTextDictionary, CurrentOperationDictionary, IsStartAddedObjectDictionary);

            if (inputText.Contains(GeneralCommands.Назад.ToString()))
            {
                await PrintInline("Ти впевнений(а) що хочеш завершити туторчик?", chatId, SetupInLine("Так", "tutorialComplete"));
            }
            else if (inputText.Contains("tutorialComplete"))
            {
                await PrintMessage("Туторчик завершений.", chatId);
                ClearEndDataTutorial(chatId);
                isOk = true;
            }
            else if (tutorial.Contains(TutorialReminderEnum.tutorialReminder.ToString()))
            {
                await TutorialReminder(chatId, inputText, update, token);
            }
            else if (tutorial.Contains(TutorialPeriodEnum.tutorialPeriod.ToString()))
            {
                await TutorialPeriod(chatId, inputText, update, token);
            }
            else if (tutorial.Contains(TutorialNotifyTheUserEnum.tutorialNotifyTheUser.ToString()))
            {
                await TutorialNotifyTheUser(chatId, inputText, update, token);
            }

            return isOk;
        }

        private async Task TutorialReminder(long chatId, string inputText, Update update, CancellationToken token)
        {
            if (inputText == TutorialReminderEnum.tutorialReminder.ToString())
            {
                await PrintKeyboard("Я твій помічник у двох запиатаннях:\nНагадування та Період овуляції.\nПочнемо з нагадувань, тому\nНатисни Нагадування.", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), token);
                await PrintMessage("Щоб завершити навчання тисни Назад, в даному випадку потрібно зайти в Нагадування і там буде кнопка Назад.", chatId, 0);
            }
            else if (inputText == ReminderCommands.Нагадування.ToString())
            {
                await PrintKeyboard($"Перед тобою з'явились три кнопки: \nДодати, Переглянути та Назад." +
                $"\nМожливо ти вже дагадався(лась) що ці кнопки виконують, але ми зараз розглянемо детальніше." +
                $"\nНатисни Додати щоб додати нове нагадування.", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                objectControllerBase.SetDictionary(chatId, NotAllowed.AddReminder, TutorialDictionary);
            }
            else if (inputText == ReminderCommands.Додати.ToString() || ReminderCommands.Додати.ToString() == CurrentOperationDictionary[chatId])
            {
                if(TutorialDictionary[chatId] == NotAllowed.AddReminder)
                {
                    if (AddObject(chatId, inputText, ReminderCommands.Додати.ToString(), ReminderCommands.Додати.ToString(), AddReminder, token, update))
                    {
                        await PrintKeyboard("Твоє нагадування поки знаходитсья в моїй цифровій макітрі, щоб я його запам'ятав перевірь дані та натискай Зберегти.", chatId, SetupKeyboard(ReminderCommands.Додати.ToString(), ReminderCommands.Переглянути.ToString(), GeneralCommands.Назад.ToString()), token);
                        await reminderController.DisplayCurrentReminder(chatId, TutorialReminderEnum.tutorialReminderSave.ToString());
                        objectControllerBase.SetDictionary(chatId, NotAllowed.tutorialReminderSave, TutorialDictionary);
                    }
                }
                else
                {
                    await PrintMessage("Притримуйся інструкції будь ласка.", chatId);
                    return;
                }
            }
            else if (inputText == TutorialReminderEnum.tutorialReminderSave.ToString())
            {
                if(objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.tutorialReminderSave)
                {
                    if (reminderController.Save(chatId, out int reminderId))
                    {
                        await PrintMessage("Таакссс... Ну я його постараюсь не забути, а ти тепер ти можеш перевірити активні нагадування, " +
                            "та видалити їх якщо вони тобі не потрібні. Щоб зробити це" +
                            "\nНатисни Переглянути.", chatId, 2000);
                        objectControllerBase.SetDictionary(chatId, NotAllowed.DisplayReminder, TutorialDictionary);

                        IdObjectForClearDataUserTutorial.Add(chatId, reminderId);
                    }
                }
            }
            else if (inputText == ReminderCommands.Переглянути.ToString())
            {
                if (TutorialDictionary[chatId] == NotAllowed.DisplayReminder)
                {
                    objectControllerBase.Display<Reminder>(chatId, "У тебе немає нагадувань, час їх створити.", CallbackQueryCommands.deleteReminder, new DataBaseContextForReminder());
                    Task.Delay(1000).Wait();
                    await PrintInline("Ну що ж тобі сказати друже, постараюсь не забути, " +
                        "а ти поки можеш тут нууу... як тобі сказати, дивитись мої можливості далі. Так що\nТисни Далі", chatId, SetupInLine("Далі", TutorialPeriodEnum.tutorialPeriod.ToString()));
                    return;
                }

                if(TutorialDictionary[chatId] == NotAllowed.AddReminder)
                {
                    await PrintMessage("Притримуйся інструкції будь ласка, тисни Додати.", chatId);
                    return;
                }
                else if (TutorialDictionary[chatId] == NotAllowed.tutorialReminderSave)
                {
                    await PrintMessage("Притримуйся інструкції будь ласка, тисни Зберегти.", chatId);
                    return;
                }

            }
            else if (inputText == PeriodCommands.Період.ToString())
            {
                await PrintMessage("Ой... Ти переплутав(ла) кнопку, тисни лівіше.", chatId);
            }
            else if (inputText == GeneralCommands.myId.ToString())
            {
                await PrintMessage("Поки що тобі це не потрібно, тому тисни Нагадування.", chatId);
            }
            else
            {
                await PrintMessage("Щось невідоме... Перевірь та спробуй ще раз.", chatId);
            }
        }

        private async Task TutorialPeriod(long chatId, string inputText, Update update, CancellationToken token)
        {
            if (inputText == TutorialPeriodEnum.tutorialPeriod.ToString())
            {
                await PrintKeyboard("Далі у нас по списку це розділ \"Період овуляції\" скорочено Період." +
                    "\nЦей розділ створений для відстеження періоду овуляції твоєї двічини, друга і т.д., тобто він працює як звичайний календарик який є у майже кожної дівчини, але я не займаю жодного місця на твоєму смартфоні, про це турбується мій розробник, тому +1 в мою сторону, а ми йдемо далі так що швидше натискай Період і погнали.", chatId, SetupKeyboard(ReminderCommands.Нагадування.ToString(), PeriodCommands.Період.ToString(), GeneralCommands.myId.ToString()), token);
                return;
            }
            else if (inputText == ReminderCommands.Нагадування.ToString())
            {
                await PrintMessage("Тобі потрібно трішечки правіше.", chatId);
                return;
            }
            else if (inputText == PeriodCommands.Період.ToString())
            {
                await PrintKeyboard("Перед тобою з'явились три кнопки:\nПеріод, Користувачі та Назад.\nРозділ Користувачі ми роглянемо трішечки пізніше, тому лясни ти вже цю кнопку Період.", chatId, SetupKeyboard(PeriodCommands.Перiод.ToString(), NotifyTheUserCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), token);
                return;
            }
            else if (inputText == PeriodCommands.Перiод.ToString())
            {
                await PrintKeyboard("Супер, тепер перед тобою з'явились чотири кнопки: Створити, Проглянути, Розпочались та Назад." +
                    "\nНатиснувши Створити ти зможеш добавити свій цикл Овуляції, ввівши тривалість менструації, циклу, дати останьої менструації," +
                    " але навіщо я тобі це розповідаю якщо ти можеш сам(а) перевірити, хутчіш \nНатискай Створити.",
                    chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), token);

                objectControllerBase.SetDictionary(chatId, NotAllowed.AddPeriod, TutorialDictionary);
                return;
            }
            else if (inputText == PeriodCommands.Створити.ToString() || PeriodCommands.Створити.ToString() == CurrentOperationDictionary[chatId])
            {
                if (objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.AddPeriod)
                {
                    if (AddObject(chatId, inputText, PeriodCommands.Створити.ToString(), PeriodCommands.Створити.ToString(), AddPeriod, token, update))
                    {
                        await periodController.DisplayCurrentPeriod(chatId, CallbackQueryCommands.savePeriod.ToString());
                        await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), token);
                        objectControllerBase.SetDictionary(chatId, NotAllowed.DisplayPeriod, TutorialDictionary);
                    }
                    return;
                }

                await PrintMessage("Стривай... Не зараз.", chatId);
                return;
            }
            else if (inputText == TutorialPeriodEnum.tutorialPeriodSave.ToString())
            {
                if (periodController.Save(chatId, out int periodId))
                {
                    await PrintMessage("Ти ж моя розумашка, тепер ти можеш проглянути свій цикл.\nТисни Проглянути.", chatId, 2000);
                    objectControllerBase.SetDictionary(chatId, NotAllowed.DisplayPeriod, TutorialDictionary);

                    IdObjectForClearDataUserTutorial.Add(chatId, periodId);
                }
            }
            else if (inputText == PeriodCommands.Проглянути.ToString())
            {
                if (objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.DisplayPeriod)
                {
                    await PrintMessage("Тут ти можеш перевірити свій цикл та видалити його якщо захочеш щось змінити, все наче просто, тому йдемо далі.", chatId, 2000);
                    objectControllerBase.Display<Period>(chatId, "У тебе немає створеного циклу, вже самий час його створити.", CallbackQueryCommands.deletePeriod, new DataBaseContextForPeriod());
                    await PrintMessage("Кнопка Розпочались створена для того щоб розпочати відлік днів менструації, так як інколи бувають затримки або можуть раніше розпочатись \"ці дні\"." +
                        "\nДля перевірки та розуміння її роботи, натискай Розпочались(якщо ти ввів(ввела) все як було вказано вище то все буде добре, якщо ні то може бути сюрприз у вигляді помилки).", chatId, 2000);
                    objectControllerBase.SetDictionary(chatId, NotAllowed.startMenstruation, TutorialDictionary);
                    return;
                }

                await PrintMessage("Не попав... Спробуй трішечки лівіше.", chatId);
                return;
            }
            else if (inputText == PeriodCommands.Розпочались.ToString())
            {
                if (objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.startMenstruation)
                {
                    new PeriodController().Update(chatId);
                    await PrintInline("Ну ось ти й розпочав(ла) свій перший цикл, наче все просто тому йдемо далі.\nНатисни Далі.", chatId, SetupInLine("Далі", TutorialNotifyTheUserEnum.tutorialNotifyTheUser.ToString()));
                    return;

                }

                await PrintMessage("Ні, не це тобі потрібно.", chatId);
                return;
            }
            else if (inputText == NotifyTheUserCommands.Користувачі.ToString())
            {
                await PrintMessage("Йой... Ти щось промазав, тобі потрібен Період.", chatId);
                return;
            }
            else if (inputText == GeneralCommands.myId.ToString())
            {
                await PrintMessage("Не туди... Хмм... Цікава фраза.", chatId);
                return;
            }
            else
            {
                await PrintMessage("Щось невідоме... Перевірь та спробуй ще раз.", chatId);
            }
        }

        private async Task TutorialNotifyTheUser(long chatId, string inputText, Update update, CancellationToken token)
        {
            if (inputText == TutorialNotifyTheUserEnum.tutorialNotifyTheUser.ToString())
            {
                await PrintKeyboard("І саме головне, це моя фішка яку я можу тобі запропонувати(окрім того що я не займаю жодного місця на твоєму пристрої) " +
                "це сповішувати про твій цикл користувачів, яких ти добавиш вівши їх ідентифікатор чату та ім'я яке будеш бачити тільки ти, " +
                "тому ти можеш ввести будь яке ім'я і ніхто про це не дізнається." +
                "\nДля того щоб твої корситувачі отримували повідомлення вони повинні зробити декілька простих кроків, це запустити бота(натиснути start), " +
                "та відправити тобі свій Chat Id який можна дізнатись на початку додатку та у цьому розділі." +
                "\nТому тисни Користувачі і продовжимо твоє навчання.", chatId, SetupKeyboard(PeriodCommands.Перiод.ToString(), NotifyTheUserCommands.Користувачі.ToString(), GeneralCommands.Назад.ToString()), token);

                return;
            }
            else if (inputText == PeriodCommands.Перiод.ToString())
            {
                await PrintMessage("Друже, натисни правіше...", chatId);
                return;
            }
            else if (inputText == NotifyTheUserCommands.Користувачі.ToString())
            {
                objectControllerBase.SetDictionary(chatId, NotAllowed.ClickMyIdNotifyTheUser, TutorialDictionary);
                await PrintKeyboard("Перед тобою чотири кнопки:\nДобавити, Продивитись, MyId та Назад." +
                    "Все впринципі працює та виглядає як те, що ти бавчи раніше, тому я думаю у тебе не виникнуть проблеми з цим, але перед тим як добавити свого першого користувача(в даному випадку ти добавиш сам себе), дізнайся свій Chat Id." +
                    "\nНатисни MyId.", chatId, SetupKeyboard(NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Продивитись.ToString(), GeneralCommands.myId.ToString(), GeneralCommands.Назад.ToString()), token);
                return;
            }
            else if (inputText == GeneralCommands.myId.ToString())
            {
                if (objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.ClickMyIdNotifyTheUser)
                {
                    await PrintMessage($"Id твого чату:", chatId);
                    await PrintMessage($"{chatId}", chatId);

                    await PrintMessage("Тепер натисни Добавити, та дотримуйся інструкції.", chatId);

                    objectControllerBase.SetDictionary(chatId, NotAllowed.addNotifyTheUser, TutorialDictionary);
                    return;
                }

                await PrintMessage("Не зараз... Дотримуйя інструкції.", chatId);
                return;
            }
            else if (inputText == NotifyTheUserCommands.Добавити.ToString() || NotifyTheUserCommands.Добавити.ToString() == CurrentOperationDictionary[chatId])
            {
                if (objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.addNotifyTheUser)
                {
                    if (AddObject(chatId, inputText, NotifyTheUserCommands.Добавити.ToString(), NotifyTheUserCommands.Добавити.ToString(), AddNotifyTheUser, token, update))
                    {
                        await PrintMessage("Принцип точно такий самий, перевіряєш та відправляєш запрошення користувачу, і після того як він його прийме, йому будуть приходити новини про твій цикл.\nНатискай Запросити.", chatId);
                        await notifyTheUserController.DisplayCurrentNotifyTheUser(chatId, TutorialNotifyTheUserEnum.tutorialNotifyTheUserInviteSend.ToString());
                    }
                    return;
                }

                await PrintMessage("Не та кнопка, читай уважніше вказання.", chatId);
                return;
            }
            else if (inputText == TutorialNotifyTheUserEnum.tutorialNotifyTheUserInviteSend.ToString())
            {
                await PrintMessage("Ось так виглядає твоє запрошення коли ти його відправляєш користувачу. Щоб прийняти запрошення \nНатисни Підтвердити.", chatId);
                await notifyTheUserController.SendInvite(chatId, update, TutorialNotifyTheUserEnum.tutorialNotifyTheUserConfirmInvite.ToString());
            }
            else if (inputText == TutorialNotifyTheUserEnum.tutorialNotifyTheUserConfirmInvite.ToString())
            {
                if (notifyTheUserController.Save(chatId, out int notifyTheUserId))
                {
                    await PrintMessage("Вітаю... Тепер у тебе є перший добавлений користувач, далі що тобі потрібно зробити це продивитись своїх користувачів. \nНатисни Продивитись.", chatId);
                    objectControllerBase.SetDictionary(chatId, NotAllowed.DisplayNotifyTheUser, TutorialDictionary);

                    IdObjectForClearDataUserTutorial.Add(chatId, notifyTheUserId);
                }
            }
            else if (inputText == NotifyTheUserCommands.Продивитись.ToString())
            {
                if (objectControllerBase.GetDictionary(chatId, TutorialDictionary) == NotAllowed.DisplayNotifyTheUser)
                {
                    objectControllerBase.Display<Period>(chatId, "У тебе немає доданих користувачів, мені здається самий час їх добавити.", CallbackQueryCommands.deleteNotifyTheUser, new DataBaseContextForPeriod());
                    Task.Delay(2000).Wait();

                    await PrintMessage("Ехх... Ти так швидко все запамятовуєш що я навіть не встиг додати 2 + 2." +
                        "\nНа цьому туторчик закінчився, тому я та мій розробник бажаємо тобі не забувати нічого що б не створювати Нагадування, та безболісного циклу.", chatId, 2000);

                    new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load().Where(x => x.ChatId == chatId).ToList().ForEach(x =>
                      {
                          new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Remove(x);
                      });
                    return;
                }

                await PrintMessage("Будь ласка дотримуйся інструкції.", chatId);
                return;
            }
            else
            {
                await PrintMessage("Невідома команда...", chatId);
            }

        }

        #endregion

        #region ClearEndDataTutorial

        public void ClearEndDataTutorial(long chatId)
        {
            Clear<Reminder>(new DataBaseContextForReminder(), chatId);
            Clear<Period>(new DataBaseContextForPeriod(), chatId);
            Clear<NotifyTheUser>(new DataBaseContextForPeriod(), chatId);
        }

        private void Clear<G>(DbContext context, long chatId) where G : ModelBase
        {
            if (IdObjectForClearDataUserTutorial.ContainsKey(chatId))
            {
                DataBaseControllerBase<G> dataBaseControllerBase = new(context);

                dataBaseControllerBase.Load().Where(x=>x.Id == IdObjectForClearDataUserTutorial[chatId]).ToList().ForEach(async x =>
                {
                    dataBaseControllerBase.Remove(x);
                });
            }
        }

        #endregion

        #region AddObject

        private bool AddReminder(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (reminderController.Add(InputTextDictionary[chatId], AddReminderDictionary[chatId], chatId, cancellationToken, out AddReminderDictionary, out NavigationDictionary))
                return true;
            else
                return false;
        }

        private bool AddPeriod(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (periodController.Add(InputTextDictionary[chatId], AddPeriodDictionary[chatId], chatId, cancellationToken, out AddPeriodDictionary, out NavigationDictionary, out bool isExistPeriodOut))
            {
                if (isExistPeriodOut)
                {
                    objectControllerBase.SetDictionary(chatId, "", CurrentOperationDictionary);
                    return false;
                }
                return true;
            }
            else
                return false;
        }

        private bool AddNotifyTheUser(long chatId, CancellationToken cancellationToken, Update update)
        {
            if (notifyTheUserController.Add(InputTextDictionary[chatId], AddNotifyTheUserDictionary[chatId], update, chatId, cancellationToken, out AddNotifyTheUserDictionary, out NavigationDictionary))
                return true;
            else
                return false;
        }

        private bool AddObject(long chatId, string messageText, string replaceText, string buttonName, Func<long, CancellationToken, Update, bool> deleagate, CancellationToken cancellationToken, Update update)
        {
            bool isOk = false;
            if (IsStartAddedObjectDictionary[chatId])
            {
                objectControllerBase.SetDictionary(chatId, AddReminderEnum.addReminder, AddReminderDictionary);
                objectControllerBase.SetDictionary(chatId, AddPeriodEnum.addPeriod, AddPeriodDictionary);
                objectControllerBase.SetDictionary(chatId, AddNotifyTheUserEnum.addNotifyTheUser, AddNotifyTheUserDictionary);
                objectControllerBase.SetDictionary(chatId, NavigationEnum.AddReminder, NavigationDictionary);
                objectControllerBase.SetDictionary(chatId, false, IsStartAddedObjectDictionary);
            }

            if (InputTextDictionary.ContainsKey(chatId))
                InputTextDictionary[chatId] += messageText.Replace(replaceText, "");

            if (AddReminderDictionary.ContainsKey(chatId)
            && InputTextDictionary.ContainsKey(chatId))
            {
                if (deleagate.Invoke(chatId, cancellationToken, update))
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

        #endregion

        public async Task<bool> CheckTutorial(long chatId, string messageText, string tutorialUser, Update update, CancellationToken cancellationToken)
        {
            bool isOk = false;

            objectControllerBase.SetDictionary(chatId, tutorialUser, TutorialUserDictionary);

            if (TutorialUserDictionary.ContainsKey(chatId))
            {
                if (await Tutorial(chatId, messageText, TutorialUserDictionary[chatId], update, cancellationToken))
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
