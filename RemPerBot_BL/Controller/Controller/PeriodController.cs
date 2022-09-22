using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Controller.Controller;
using System.Data;
using System.Runtime.ConstrainedExecution;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class PeriodController : BotControllerBase
    {
        #region Variable

        Dictionary<long, int> MenstruationDictionary = new();
        Dictionary<long, int> CycleDictionary = new();
        Dictionary<long, DateTime> DateOfLastMenstruationDictionary = new();
        Dictionary<long, int> ClearDictionaryUser = new();
        ObjectControllerBase objectControllerBase = new();

        #endregion

        public bool Add(string inputText, AddPeriodEnum addPeriodEnum, long chatId, CancellationToken token, out Dictionary<long, AddPeriodEnum> AddPeriodDictionary, out Dictionary<long, NavigationEnum> NavigationDictionary, out bool isExistPeriodOut)
        {
            bool isExistPerOut = false;

            AddPeriodDictionary = new Dictionary<long, AddPeriodEnum>();
            NavigationDictionary = new Dictionary<long, NavigationEnum>();

            bool isOK = CheckForSavePeriod(chatId, inputText, addPeriodEnum, out AddPeriodEnum addPeriodEnumOut, out NavigationEnum navigationEnumOut, out isExistPerOut, token);

            objectControllerBase.SetDictionary(chatId, addPeriodEnumOut, AddPeriodDictionary);
            objectControllerBase.SetDictionary(chatId, navigationEnumOut, NavigationDictionary);
            isExistPeriodOut = isExistPerOut;
            return isOK;
        }

        /// <summary>
        /// Gets the data and stores the period.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="text">Data.</param>
        /// <param name="outputText">Output data.</param>
        /// <param name="button">Navigation button.</param>
        /// <param name="token">Token.</param>
        private bool CheckForSavePeriod(long chatId, string text, AddPeriodEnum addPeriodEnum,out AddPeriodEnum addPeriodEnumOut, out NavigationEnum navigationEnum, out bool isExistPeriodOut, CancellationToken token)
        {
            NavigationEnum navEnum = NavigationEnum.PeriodMenu;
            AddPeriodEnum addPerEnum = AddPeriodEnum.addPeriod;
            bool isOk = false;
            bool isExistPerOut = false;

            var periods = new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load();


            Task task = Task.Run(async () =>
            {
                // If the period has already been created, this will be done
                foreach (var period in periods)
                {
                    if(period.ChatId == chatId)
                    {
                        addPerEnum = AddPeriodEnum.empty;
                        await PrintInline("У тебе вже є створений цикл, якщо ти хочеш щось змінити то просто видали його...", period.ChatId, SetupInLine("Видалити", CallbackQueryCommands.deletePeriod.ToString() + " " + period.Id));
                        isExistPerOut = true;
                        isOk = true;
                        return;
                    }
                }

                navEnum = NavigationEnum.addPeriod;

                if (addPeriodEnum == AddPeriodEnum.addPeriod)
                {
                    addPerEnum = AddPeriodEnum.addPeriodMenstruation;
                    await PrintKeyboard("Введи тривалість менструації\nПриклад: 7", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    return;
                }
                else if (addPeriodEnum == AddPeriodEnum.addPeriodMenstruation)
                {
                    addPerEnum = AddPeriodEnum.addPeriodMenstruation;

                    text = text.Replace(AddPeriodEnum.addPeriodMenstruation.ToString(), "");
                    
                    if (!int.TryParse(text, out int durationMenstruatin))
                    {
                        await PrintMessage("Щось не так з тривалістю менструації...\nСпробуй ще раз...", chatId);
                        return;
                    }                
                    else if (durationMenstruatin <= 0)
                    {
                        await PrintMessage("Тривалість ментсруації не може бути 0...\nСпробуй ще раз...", chatId);
                        return;
                    }
                    else if (durationMenstruatin < 4 || durationMenstruatin > 10)
                    {
                        await PrintMessage("Тривалість ментсруації не може бути менше 4, та быльше 10 днів...\nСпробуй ще раз...", chatId);
                        return;
                    }

                    objectControllerBase.SetDictionary(chatId, durationMenstruatin, MenstruationDictionary);
                    await PrintKeyboard("Введи тривалість циклу\nПриклад: 30", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    addPerEnum = AddPeriodEnum.addPeriodCycle;
                }
                else if (addPeriodEnum == AddPeriodEnum.addPeriodCycle)
                {
                    addPerEnum = AddPeriodEnum.addPeriodCycle;
                    
                    text = text.Replace(AddPeriodEnum.addPeriodCycle.ToString(), "");
                    
                    if (!int.TryParse(text, out int durationCycle))
                    {
                        await PrintMessage("Щось не так з тривалістю циклу...\nСпробуй ще раз...", chatId);
                        return;
                    }

                    if (durationCycle <= 0)
                    {
                        await PrintMessage("Тривалість циклу не може бути 0...\nСпробуй ще раз...", chatId);
                        return;
                    }
                    else if (durationCycle < 15 || durationCycle > 40)
                    {
                        await PrintMessage("Тривалість циклу не може бути менше 15, та більше 40 днів...\nСпробуй ще раз...", chatId);
                        return;
                    }

                    objectControllerBase.SetDictionary(chatId, durationCycle, CycleDictionary);

                    await PrintKeyboard($"Введи дату останьої менструації\nПриклад: {DateTime.Today.AddDays(3).AddMonths(-1).ToShortDateString()}", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    addPerEnum = AddPeriodEnum.addPeriodDateOfLastMenstruationMenstruation;
                }
                else if (addPeriodEnum == AddPeriodEnum.addPeriodDateOfLastMenstruationMenstruation)
                {
                    addPerEnum = AddPeriodEnum.addPeriodDateOfLastMenstruationMenstruation;
                    text = text.Replace(AddPeriodEnum.addPeriodDateOfLastMenstruationMenstruation.ToString(), "");
                    
                    if (!DateTime.TryParse(text, out DateTime dateOfLastMenstruation))
                    {
                        await PrintMessage("Щось не так з датою останьої менструації...\nСпробуй ще раз...", chatId);
                        return;
                    }

                    if (dateOfLastMenstruation == DateTime.MinValue)
                    {
                        await PrintMessage("Дата останьої менструації не може бути 0...\nСпробуй ще раз...", chatId);
                        return;
                    }
                    else if (dateOfLastMenstruation > DateTime.Today)
                    {
                        await PrintMessage("Дата останьої менструації не може бути з майбутнього...\nСпробуй ще раз...", chatId);
                        return;
                    }
                    else if (dateOfLastMenstruation < DateTime.Today.AddDays(-CycleDictionary[chatId]))
                    {
                        await PrintMessage($"Дата останьої менструації не може бути більше {CycleDictionary[chatId]} днів тому...\nСпробуй ще раз...", chatId);
                        return;
                    }

                    objectControllerBase.SetDictionary(chatId, dateOfLastMenstruation, DateOfLastMenstruationDictionary);

                    isOk = true;
                }
            });

            task.Wait();

            addPeriodEnumOut = addPerEnum;
            navigationEnum = navEnum;
            isExistPeriodOut = isExistPerOut;
            return isOk;
        }


        public async Task DisplayCurrentPeriod(long chatId, string callbackName)
        {
            MenstruationDictionary.TryGetValue(chatId, out int menstruationDictionaryValue);
            CycleDictionary.TryGetValue(chatId, out int cycleDictionaryValue);
            DateOfLastMenstruationDictionary.TryGetValue(chatId, out DateTime dateOfLastMenstruationDictionaryValue);
            await PrintInline($"Ваші налаштування:\nТривалість менструації: {menstruationDictionaryValue} д.\nТривалість циклу: {cycleDictionaryValue} д.\nДата останьої менструації: {dateOfLastMenstruationDictionaryValue}", chatId, SetupInLine(CallbackQueryCommands.Зберегти.ToString(), callbackName));
        }
        
        /// <summary>
        /// Checks for null and saves the period.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="durationMenstruation">Duratiom menstruation.</param>
        /// <param name="durationСycle">Duration cycle.</param>
        /// <param name="dateOfLastMenstruation">Date of last menstruation.</param>
        public bool Save(long chatId, out int periodId, int menstruationDictionaryValue = 0, int cycleDictionaryValue = 0, DateTime dateOfLastMenstruationDicionaryValue = default)
        {
            bool isOk = false;
            int perId = 0;

            if (menstruationDictionaryValue == 0 && cycleDictionaryValue == 0 && dateOfLastMenstruationDicionaryValue == DateTime.MinValue)
            {
                MenstruationDictionary.TryGetValue(chatId, out menstruationDictionaryValue);
                CycleDictionary.TryGetValue(chatId, out cycleDictionaryValue);
                DateOfLastMenstruationDictionary.TryGetValue(chatId, out dateOfLastMenstruationDicionaryValue);
            }

            if (menstruationDictionaryValue != 0 && cycleDictionaryValue != 0 && dateOfLastMenstruationDicionaryValue != DateTime.MinValue)
            {
                if (new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Save(new Period(chatId, menstruationDictionaryValue, cycleDictionaryValue, dateOfLastMenstruationDicionaryValue, dateOfLastMenstruationDicionaryValue.AddDays(cycleDictionaryValue))))
                {
                    perId = new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load().Where(per => per.ChatId == chatId).Last().Id;

                    MenstruationDictionary.Remove(chatId);
                    CycleDictionary.Remove(chatId);
                    DateOfLastMenstruationDictionary.Remove(chatId);
                    isOk = true;
                }
            }

            periodId = perId;
            return isOk;
        }

        /// <summary>
        /// Checks when the menstrual cycle will start.
        /// </summary>
        public void CheckPeriodThread()
        {
            List<Period> Periods = new();

            Task checkPeriod = Task.Run(() =>
            {
                while (true)
                {
                    // Gets all periods and lists of users to send messages to.
                    using (DataBaseContextForPeriod db = new())
                    {
                        Periods = db.Periods.Include(n => n.NotifyTheUser).ToList();
                    }

                    // Checks all cycles and, if necessary, sends a text to the user and his friends.
                    CheckPeriodDay(-7, Periods, "Муки через 7 днів.", "В неї Муки через 7 днів.");
                    CheckPeriodDay(-3, Periods, "Муки через 3 дні.", "В неї Муки через 3 дні.");
                    CheckPeriodDay(-2, Periods, "Муки через 2 дні.", "В неї Муки через 2 дні.");
                    CheckPeriodDay(-1, Periods, "Муки через 1 день.", "В неї Муки через 1 день.");
                    CheckPeriodDay(0, Periods, "Сьогодні перший день за графіком, як тільки розпочнеться цикл натисни \"Розпочались\".", "В неї сьогодні перший день за графіком, як тільки розпочнеться цикл я тобі повідомлю.");
                    CheckPeriodDay(1, Periods, "Перший день затримки.", "В неї Сьогодні перший день затримки.");
                    CheckPeriodDay(2, Periods, "Другий день затримки.", "В неї Сьогодні другий день затримки.");
                    CheckPeriodDay(3, Periods, "Третій день затримки.", "В неї Сьогодні третій день затримки.");
                    Thread.Sleep(1000);
                }
            });
        }

        /// <summary>
        /// Checks how many days until the start of the cycle, and sends a message with the result to the user and his friends.
        /// </summary>
        /// <param name="day">Number of days to check.</param>
        /// <param name="periods">All periods.</param>
        /// <param name="textMsg">Message text.</param>
        /// <param name="textMsgNotify">Message text for friends.</param>
        private void CheckPeriodDay(int day, List<Period> periods, string textMsg, string textMsgNotify)
        {
            periods.Where(p => p.isNotify == false).Where(x => x.DateOfNextMenstruation.AddDays(day) == DateTime.Today).ToList()
                               .ForEach(async p =>
                               {
                                   // send text for user.
                                   p.isNotify = true;
                                   new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Update(p);
                                   await PrintMessage(textMsg, p.ChatId);

                                   // send text for friends.
                                   p.NotifyTheUser.ForEach(async (n) =>
                                   {
                                       if (p.ChatId == n.ChatId)
                                       {
                                           await PrintMessage(textMsgNotify, n.ChatIdAdded);
                                       }
                                   });
                               });
        }

        /// <summary>
        /// Changes the status of isNotify to false every 24 hours, so that the user can be notified again about any changes in his cycle.
        /// </summary>
        public void ChangeIsNotifyThread()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load().ForEach(p =>
                    {
                        p.isNotify = false;
                        new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Update(p);
                    });

                    Thread.Sleep(10000);
                }
            });
        }

        /// <summary>
        /// Starts the user's period and sets the date of the next cycle.
        /// </summary>
        /// <param name="chatId"></param>
        public async void Update(long chatId)
        {
            bool isOK = false;
            var periods = new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Load().Where(p => p.ChatId == chatId).ToList();

            if (periods.Count != 0)
            {
                if (periods.First().DurationMenstruationVariable != -1)
                {
                    await PrintMessage("Твій цикл вже розпочався.", chatId);
                    return;
                }

                for (int i = 0; i < 8; i++)
                {
                    if (periods.First().DateOfNextMenstruation.AddDays(-i) == DateTime.Today)
                    {
                        periods.Where(p => p.ChatId == chatId).ToList().ForEach(async p =>
                        {
                            if (p.DurationMenstruationVariable != -1)
                            {
                                await PrintMessage("Цикл вже розпочався.", p.ChatId);
                                return;
                            }
                            p.DateOfNextMenstruation = DateTime.Today;
                            p.isNotify = false;
                            p.DurationMenstruationVariable = p.DurationMenstruation;
                            p.DateOfLastMenstruation = DateTime.Today;
                            p.DateOfNextMenstruation = DateTime.Today.AddDays(p.DurationСycle);
                            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Update(p);
                            isOK = true;
                        });
                    }
                }

                if (!isOK) 
                {
                    await PrintMessage("Менструація не може початись раніше ніж за 7 днів до планової менструації.", chatId);
                }

            }
            else
                await PrintMessage("Цикл не знайдено,саме час його створити.", chatId);
        }

        /// <summary>
        /// Shows how many days of menstruation are left.
        /// </summary>
        public void StartMenstruationThread()
        {
            Task.Run(() =>
            {
                List<Period> Periods = new();
                while (true)
                {
                    using(DataBaseContextForPeriod db = new())
                    {
                        Periods = db.Periods.Include(n => n.NotifyTheUser).ToList();
                    }

                    Periods.Where(p => p.isNotify == false).Where(period => period.DurationMenstruationVariable >= 1).ToList().ForEach(async period =>
                      {
                          period.isNotify = true;
                          period.DurationMenstruationVariable -= 1;
                          new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Update(period);
                          await PrintMessage($"Ще всього лиш декілька({period.DurationMenstruationVariable + 1}) днів.", period.ChatId);
                          period.NotifyTheUser.ForEach(async n =>
                          {
                              await PrintMessage($"Терпіти цей жах ще декілька({period.DurationMenstruationVariable + 1}) днів.", n.ChatId);
                          });
                      });

                    Periods.Where(p => p.isNotify == false).Where(p => p.DurationMenstruationVariable == 0).ToList().ForEach(async p =>
                        {
                            p.DateOfLastMenstruation = DateTime.Today;
                            p.DateOfNextMenstruation = DateTime.Today.AddDays(p.DurationСycle);
                            p.DurationMenstruationVariable = -1;
                            p.isNotify = true;
                            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).Update(p);
                            await PrintMessage($"Цикл закінчився.\nНаступний період: {p.DateOfNextMenstruation}.", p.ChatId);
                            p.NotifyTheUser.ForEach(async n =>
                            {
                                await PrintMessage($"Цикл закінчився.\nНаступний період: {p.DateOfNextMenstruation}.", n.ChatId);
                            });
                        });
                    Task.Delay(1000).Wait();
                }
            });
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
                    MenstruationDictionary.Remove(item.Key);
                    CycleDictionary.Remove(item.Key);
                    DateOfLastMenstruationDictionary.Remove(item.Key);
                    ClearDictionaryUser.Remove(item.Key);
                }
            }
        }
    }
}
