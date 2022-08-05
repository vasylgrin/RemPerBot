using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using System.Data;
using Telegram.Bot.Types;

namespace MySuperUniversalBot_BL.Controller
{
    public class PeriodController : BotControllerBase
    {
        Dictionary<long, int> MenstruationDictionary = new();
        Dictionary<long, int> CycleDictionary = new();
        Dictionary<long, DateTime> DateOfLastMenstruationDictionary = new();

        /// <summary>
        /// Gets the data and stores the period.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="text">Data.</param>
        /// <param name="outputText">Output data.</param>
        /// <param name="button">Navigation button.</param>
        /// <param name="token">Token.</param>
        public void AddPeriod(long chatId, string text, out string outputText, out byte button, CancellationToken token)
        {
            byte but = 4;
            const string addPeriod = "addPeriod ";
            const string addPeriodCycle = "addPeriodCycle ";
            const string addPeriodMenstruation = "addPeriodMenstruation ";
            const string addPeriodDateOfLastMenstruationMenstruation = "addPeriodDateOfLastMenstruationMenstruation ";

            string output = addPeriod;

            Task task = Task.Run(async () =>
            {
                new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).LoadDB(out List<Period> periods);

                // If the period has already been created, this will be done.
                foreach (var period in periods)
                {
                    if (period.ChatId == chatId)
                    {
                        output = "";
                        await PrintMessage("У тебе вже є створений цикл, якщо ти хочеш щось змінити то просто видали його...", period.ChatId);
                        return;
                    }
                }

                but = 5;
                string[] word = text.Trim().Split(new char[] { ' ' });
                word[0] += " ";
                
                if (word[0]==addPeriod)
                {
                    output = addPeriodMenstruation;
                    await PrintKeyboard("Введи тривалість менструації\nПриклад: 7", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    return;
                }
                else if (word[0] == addPeriodMenstruation)
                {
                    output = addPeriodMenstruation;
                    text = text.Replace(addPeriodMenstruation,"");
                    if (!int.TryParse(text, out int durationMenstruatin))
                    {
                        await PrintMessage("Щось не так з тривалістю менструації...", chatId);
                        return;
                    }
                    
                    if (durationMenstruatin <= 0)
                    {
                        await PrintMessage("Тривалість ментсруації не може бути 0...", chatId);
                        return;
                    }
                    else if (durationMenstruatin < 4 || durationMenstruatin > 10)
                    {
                        await PrintMessage("Тривалість ментсруації не може бути менше 4, та быльше 10 днів...", chatId);
                        return;
                    }

                    SetDictionary(chatId, durationMenstruatin, MenstruationDictionary);

                    await PrintKeyboard("Введи тривалість циклу\nПриклад: 30", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    output = addPeriodCycle;
                }
                else if (word[0] == addPeriodCycle)
                {
                    output = addPeriodCycle;
                    text = text.Replace(addPeriodCycle, "");
                    if (!int.TryParse(text, out int durationCycle))
                    {
                        await PrintMessage("Щось не так з тривалістю циклу...", chatId);
                        return;
                    }

                    if (durationCycle <= 0)
                    {
                        await PrintMessage("Тривалість циклу не може бути 0...", chatId);
                        return;
                    }
                    else if (durationCycle < 15 || durationCycle > 40)
                    {
                        await PrintMessage("Тривалість циклу не може бути менше 15, та більше 40 днів...", chatId);
                        return;
                    }

                    SetDictionary(chatId, durationCycle, CycleDictionary);

                    await PrintKeyboard("Введи дату останьої менструації\nПриклад: 17.09.2021", chatId, SetupKeyboard(GeneralCommands.Назад.ToString()), token);
                    output = addPeriodDateOfLastMenstruationMenstruation;
                }
                else if (word[0] == addPeriodDateOfLastMenstruationMenstruation)
                {
                    output = addPeriodDateOfLastMenstruationMenstruation;
                    text = text.Replace(addPeriodDateOfLastMenstruationMenstruation, "");
                    if (!DateTime.TryParse(text, out DateTime dateOfLastMenstruation))
                    {
                        await PrintMessage("Щось не так з датою останьої менструації...", chatId);
                        return;
                    }

                    if (dateOfLastMenstruation == DateTime.MinValue)
                    {
                        await PrintMessage("Дата останьої менструації не може бути 0...", chatId);
                        return;
                    }
                    else if (dateOfLastMenstruation > DateTime.Today)
                    {
                        await PrintMessage("Дата останьої менструації не може бути з майбутнього...", chatId);
                        return;
                    }
                    else if (dateOfLastMenstruation < DateTime.Today.AddDays(-CycleDictionary[chatId]))
                    {
                        await PrintMessage($"Дата останьої менструації не може бути більше {CycleDictionary[chatId]} днів тому...", chatId);
                        return;
                    }

                    SetDictionary(chatId, dateOfLastMenstruation, DateOfLastMenstruationDictionary);

                    await PrintInline($"Ваші налаштування:\nТривалість менструації: {MenstruationDictionary[chatId]} д.\nТривалість циклу: {CycleDictionary[chatId]} д.\nДата останьої менструації: {DateOfLastMenstruationDictionary[chatId]}", chatId, SetupInLine(CallbackQueryCommands.Зберегти.ToString(), CallbackQueryCommands.savePeriod.ToString()), token);
                    await PrintKeyboard("Обирай:", chatId, SetupKeyboard(PeriodCommands.Створити.ToString(), PeriodCommands.Проглянути.ToString(), PeriodCommands.Розпочались.ToString(), GeneralCommands.Назад.ToString()), token);                    
                }
            });

            task.Wait();

            button = but;
            outputText = output;
        }   

        /// <summary>
        /// Checks for null and saves the period.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="durationMenstruation">Duratiom menstruation.</param>
        /// <param name="durationСycle">Duration cycle.</param>
        /// <param name="dateOfLastMenstruation">Date of last menstruation.</param>
        public async void SavePeriodAsync(long chatId)
        {
            if (new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).SaveDB(new Period(chatId, MenstruationDictionary[chatId], CycleDictionary[chatId], DateOfLastMenstruationDictionary[chatId], DateOfLastMenstruationDictionary[chatId].AddDays(CycleDictionary[chatId]))))
            {
                await PrintMessage($"Налаштування успішно збережено.\nТвій наступний цикл {DateOfLastMenstruationDictionary[chatId].AddDays(CycleDictionary[chatId])}", chatId);           
                MenstruationDictionary.Remove(chatId);
                CycleDictionary.Remove(chatId);
                DateOfLastMenstruationDictionary.Remove(chatId);
            }
            else
                await PrintMessage("Налаштування не збережено.", chatId);
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
                    CheckPeriodDay(-1, Periods, "Муки через 1 день.", "В неї Муки через 1 день.");
                    CheckPeriodDay(0, Periods, "Сьогодні перший день за графіком, як тільки розпочнеться цикл натисни \"Розпочались\".", "В неї Сьогодні перший день за графіком, як тільки розпочнеться цикл натисни \"Розпочались\".");

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
                                   new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).UpdateDB(p);
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
            Task changeIsNotify = Task.Run(() =>
            {
                while (true)
                {
                    new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).LoadDB().ForEach(p =>
                    {
                        p.isNotify = false;
                        new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).UpdateDB(p);
                    });

                    Thread.Sleep(86400000);
                }
            });
        }

        /// <summary>
        /// Starts the user's period and sets the date of the next cycle.
        /// </summary>
        /// <param name="chatId"></param>
        public async void UpdatePeriodAsync(long chatId)
        {
            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).LoadDB(out List<Period> periods);
            periods = periods.Where(p => p.ChatId == chatId).ToList();

            if (periods.Count != 0)
            {
                if (periods.First().DateOfNextMenstruation.AddDays(-3) == DateTime.Today)
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
                        new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).UpdateDB(p);
                    });
                }
                else
                    await PrintMessage("Ментсруація не може початись раніше ніж за 7 днів до планової менстрації.", chatId);
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
                          new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).UpdateDB(period);
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
                            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).UpdateDB(p);
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

        /// <summary>
        /// Returns the period of the user.
        /// </summary>
        /// <param name="chatId">Chat id.</param>
        /// <param name="cancellationToken">Token.</param>
        public async void DisplayPeriod(long chatId, CancellationToken cancellationToken)
        {
            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).LoadDB(out List<Period> period);

            period = period.Where(per => per.ChatId == chatId).ToList();

            if (period.Count != 0)
            {
                period.ForEach(async p =>
                {
                    await PrintInline($"{p.DurationMenstruation} {p.DurationСycle} {p.DateOfLastMenstruation}", chatId, SetupInLine(CallbackQueryCommands.Видалити.ToString(), CallbackQueryCommands.deletePeriod.ToString() + p.Id), cancellationToken);
                });
            }
            else
            {
                await PrintMessage("У тебе немає створеного циклу, вже самий час його створити.", chatId);
            }
        }

        /// <summary>
        /// Gets a callback and deletes a menstrual cycle.
        /// </summary>
        /// <param name="callbackQuery"></param>
        public async void DeletePeriod(CallbackQuery callbackQuery)
        {
            string messageText = callbackQuery.Data.Replace(CallbackQueryCommands.deletePeriod.ToString(), "");

            new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).LoadDB(out List<Period> periods);

            foreach (var period in periods)
            {
                if (period.Id == Convert.ToInt32(messageText))
                {
                    if (new DataBaseControllerBase<Period>(new DataBaseContextForPeriod()).RemoveDB(period))
                        await PrintMessage("Цикл видалено.", callbackQuery.Message.Chat.Id);
                    else
                        await PrintMessage("Цикл не видалено.", callbackQuery.Message.Chat.Id);
                    return;
                }
            }
        }
    }
}
