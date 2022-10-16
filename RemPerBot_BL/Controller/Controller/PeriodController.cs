using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Controller.ControllerBase;
using MySuperUniversalBot_BL.Models;
using Newtonsoft.Json.Linq;
using RemBerBot_BL.Controller.Controller;
using RemBerBot_BL.Controller.DataBase;
using RemBerBot_BL.Controller.Interface;
using System.Data;
using static MySuperUniversalBot_BL.Controller.BotControllerBase;

namespace MySuperUniversalBot_BL.Controller
{
    public class PeriodController : ObjectControllerBase, IObjectControllerBase
    {
        #region Variable

        Dictionary<long, int> MenstruationDictionary = new();
        Dictionary<long, int> CycleDictionary = new();
        Dictionary<long, DateTime> DateOfLastMenstruationDictionary = new();

        Dictionary<long, int> ClearDictionaryUser = new();

        BotControllerBase botControllerBase = new();

        #endregion

        public async Task<bool> Add(string inputText, long chatId, CancellationToken cancellationToken)
        {
            bool isOk = false;

            await Task.Run(() =>
            {
                OperationEnum operationEnum = DictionaryController.OperationDictionary[chatId];
                if (operationEnum == OperationEnum.addPeriod)
                {
                    botControllerBase.PrintKeyboard("Введи тривалість менструації\nПриклад: 7", chatId, botControllerBase.SetupKeyboard(GeneralCommands.Назад.ToString()), cancellationToken);
                    ClearDictionaryUser.SetDictionary(chatId, 5);

                    DictionaryController.OperationDictionary[chatId] = OperationEnum.addPeriodMenstruation;
                    DictionaryController.NavigationDictionary[chatId] = NavigationEnum.addPeriod;
                }
                else if (operationEnum == OperationEnum.addPeriodMenstruation)
                {
                    if (!int.TryParse(inputText, out int durationMenstruatin))
                        botControllerBase.PrintMessage("Щось не так з тривалістю менструації...\nСпробуй ще раз...", chatId);
                    else if (durationMenstruatin <= 0)
                        botControllerBase.PrintMessage("Тривалість ментсруації не може бути 0...\nСпробуй ще раз...", chatId);
                    else if (durationMenstruatin < 4 || durationMenstruatin > 10)
                        botControllerBase.PrintMessage("Тривалість ментсруації не може бути менше 4, та быльше 10 днів...\nСпробуй ще раз...", chatId);
                    else
                    {
                        botControllerBase.PrintKeyboard("Введи тривалість циклу\nПриклад: 30", chatId, botControllerBase.SetupKeyboard(GeneralCommands.Назад.ToString()), cancellationToken);
                        MenstruationDictionary.SetDictionary(chatId, durationMenstruatin);

                        DictionaryController.OperationDictionary[chatId] = OperationEnum.addPeriodCycle;
                    }
                }
                else if (operationEnum == OperationEnum.addPeriodCycle)
                {
                    if (!int.TryParse(inputText, out int durationCycle))
                        botControllerBase.PrintMessage("Щось не так з тривалістю циклу...\nСпробуй ще раз...", chatId);
                    else if (durationCycle <= 0)
                        botControllerBase.PrintMessage("Тривалість циклу не може бути 0...\nСпробуй ще раз...", chatId);
                    else if (durationCycle < 15 || durationCycle > 40)
                        botControllerBase.PrintMessage("Тривалість циклу не може бути менше 15, та більше 40 днів...\nСпробуй ще раз...", chatId);
                    else
                    {
                        botControllerBase.PrintKeyboard($"Введи дату останьої менструації\nПриклад: {DateTime.Today.AddDays(3).AddMonths(-1).ToShortDateString()}", chatId, botControllerBase.SetupKeyboard(GeneralCommands.Назад.ToString()), cancellationToken);
                        CycleDictionary.SetDictionary(chatId, durationCycle);

                        DictionaryController.OperationDictionary[chatId] = OperationEnum.addPeriodDateOfLastMenstruationMenstruation;
                    }
                }
                else if (operationEnum == OperationEnum.addPeriodDateOfLastMenstruationMenstruation)
                {
                    if (!DateTime.TryParse(inputText, out DateTime dateOfLastMenstruation))
                        botControllerBase.PrintMessage("Щось не так з датою останьої менструації...\nСпробуй ще раз...", chatId);
                    else if (dateOfLastMenstruation == DateTime.MinValue)
                        botControllerBase.PrintMessage("Дата останьої менструації не може бути 0...\nСпробуй ще раз...", chatId);
                    else if (dateOfLastMenstruation > DateTime.Today)
                        botControllerBase.PrintMessage("Дата останьої менструації не може бути з майбутнього...\nСпробуй ще раз...", chatId);
                    else if (dateOfLastMenstruation < DateTime.Today.AddDays(-CycleDictionary[chatId]))
                        botControllerBase.PrintMessage($"Дата останьої менструації не може бути більше {CycleDictionary[chatId]} днів тому...\nСпробуй ще раз...", chatId);
                    else
                    {
                        DateOfLastMenstruationDictionary.SetDictionary(chatId, dateOfLastMenstruation);
                        isOk = true;

                        DictionaryController.NavigationDictionary[chatId] = NavigationEnum.PeriodMenu;
                        DictionaryController.OperationDictionary[chatId] = OperationEnum.empty;
                    }
                }
            });

            return isOk;
        }

        public bool Save(long chatId, out int objectId)
        {
            objectId = 0;

            if(MenstruationDictionary.ContainsKey(chatId) 
                && CycleDictionary.ContainsKey(chatId) 
                && DateOfLastMenstruationDictionary.ContainsKey(chatId))
            {
                new DataBaseControllerBase<Period>(new RemPerContext()).Save(new Period(chatId, MenstruationDictionary[chatId], CycleDictionary[chatId], DateOfLastMenstruationDictionary[chatId], DateOfLastMenstruationDictionary[chatId].AddDays(CycleDictionary[chatId])));
                objectId = new DataBaseControllerBase<Period>(new RemPerContext()).Load().Where(period => period.ChatId == chatId).First().Id;

                MenstruationDictionary.Remove(chatId);
                CycleDictionary.Remove(chatId);
                DateOfLastMenstruationDictionary.Remove(chatId);
                ClearDictionaryUser.Remove(chatId);

                return true;
            }

            return false;
        }

        public void PrintCurrentObject(long chatId, string callbackName)
        {
            botControllerBase.PrintInline($"Ваші налаштування:\nТривалість менструації: {MenstruationDictionary[chatId]} д.\nТривалість циклу: {CycleDictionary[chatId]} д.\nДата останьої менструації: {DateOfLastMenstruationDictionary[chatId].ToShortDateString()}\nДата наступної менструації: {DateOfLastMenstruationDictionary[chatId].AddDays(CycleDictionary[chatId]).ToShortDateString()}", chatId, botControllerBase.SetupInLine(CallbackQueryCommands.Зберегти.ToString(), callbackName));
        }

        public async void SendMsgForStartPeriodAsync()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    var periods = GetIncludePeriod();
                    CheckStartPeriod(-7, periods, "Муки через 7 днів.", "В неї Муки через 7 днів.");
                    CheckStartPeriod(-3, periods, "Муки через 3 дні.", "В неї Муки через 3 дні.");
                    CheckStartPeriod(-2, periods, "Муки через 2 дні.", "В неї Муки через 2 дні.");
                    CheckStartPeriod(-1, periods, "Муки через 1 день.", "В неї Муки через 1 день.");
                    CheckStartPeriod(0, periods, "Сьогодні перший день за графіком, як тільки розпочнеться цикл натисни \"Розпочались\".", "В неї сьогодні перший день за графіком, як тільки розпочнеться цикл я тобі повідомлю.");
                    CheckStartPeriod(1, periods, "Перший день затримки.", "В неї cьогодні перший день затримки.");
                    CheckStartPeriod(2, periods, "Другий день затримки.", "В неї cьогодні другий день затримки.");
                    CheckStartPeriod(3, periods, "Третій день затримки.", "В неї cьогодні третій день затримки.");
                    Task.Delay(1000).Wait();
                }
            });
        }

        private void CheckStartPeriod(int day, List<Period> periods, string textMsg, string textMsgNotify)
        {
            periods.Where(p => p.isNotify == false)
                .Where(x => x.DateOfNextMenstruation.AddDays(day) == DateTime.Today)
                .ToList()
                .ForEach(p =>
                {
                    // send message for user.
                    p.isNotify = true;
                    new DataBaseControllerBase<Period>(new RemPerContext()).Update(p);
                    botControllerBase.PrintMessage(textMsg, p.ChatId);

                    // send message for friends.
                    p.NotifyTheUser.ForEach(n =>
                    {
                        if (p.ChatId == n.ChatId)
                            botControllerBase.PrintMessage(textMsgNotify, n.ChatIdAdded);
                    });
                });
        }

        public async void ChangeIsNotifyThread()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    new DataBaseControllerBase<Period>(new RemPerContext()).Load().ForEach(p =>
                    {
                        p.isNotify = false;
                        new DataBaseControllerBase<Period>(new RemPerContext()).Update(p);
                    });

                    Task.Delay(5000).Wait();
                }
            });
        }

        public async void Update(long chatId)
        {
            await Task.Run(() =>
            {
                bool isOK = true;
                var period = new DataBaseControllerBase<Period>(new RemPerContext()).Load().Where(p => p.ChatId == chatId).First();

                if (period != null)
                {
                    if (period.DurationMenstruationVariable != -1)
                    {
                        botControllerBase.PrintMessage("Твій цикл вже розпочався.", chatId);
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (period.DateOfNextMenstruation.AddDays(-i) == DateTime.Today)
                            {
                                period.isNotify = false;
                                period.DurationMenstruationVariable = period.DurationMenstruation;
                                period.DateOfLastMenstruation = DateTime.Today;
                                period.DateOfNextMenstruation = DateTime.Today.AddDays(period.DurationСycle);
                                new DataBaseControllerBase<Period>(new RemPerContext()).Update(period);
                                isOK = false;
                            }
                        }
                        if (isOK)
                            botControllerBase.PrintMessage("Менструація не може початись раніше ніж за 7 днів до планової менструації.", chatId);
                    }
                }
                else
                    botControllerBase.PrintMessage("Цикл не знайдено,саме час його створити.", chatId);
            });
        }

        /// <summary>
        /// Перевіряє чи існує період.
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns>true - якщо існує, false - якщо не існує.</returns>
        public bool isExistPeriod(long chatId, out int periodId)
        {
            var period = new DataBaseControllerBase<Period>(new RemPerContext()).Load().Where(p => p.ChatId == chatId).FirstOrDefault();

            if (period != null)
            {
                periodId = period.Id;
                return true;
            }
            else
            {
                periodId = 0;
                return false;
            }
        }
    
        private List<Period> GetIncludePeriod()
        {
            using (RemPerContext db = new())
            {
                return db.Periods.Include(n => n.NotifyTheUser).ToList();
            }
        }

        public async void StartMenstruationAsync()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    StartMenstruation();
                    EndMenstruation();
                    await Task.Delay(1000);
                }
            });
        }
        private void StartMenstruation()
        {
            GetIncludePeriod().Where(p => p.isNotify == false).Where(period => period.DurationMenstruationVariable >= 1).ToList().ForEach(period =>
            {
                period.isNotify = true;
                period.DurationMenstruationVariable -= 1;
                new DataBaseControllerBase<Period>(new RemPerContext()).Update(period);
                botControllerBase.PrintMessage($"Ще всього лиш декілька({period.DurationMenstruationVariable + 1}) днів.", period.ChatId);
                period.NotifyTheUser.ForEach(n =>
                {
                    botControllerBase.PrintMessage($"Терпіти цей жах ще декілька({period.DurationMenstruationVariable + 1}) днів.", n.ChatId);
                });
            });
        }
        private void EndMenstruation()
        {
            GetIncludePeriod().Where(p => p.isNotify == false).Where(p => p.DurationMenstruationVariable == 0).ToList().ForEach(p =>
            {
                p.DateOfLastMenstruation = DateTime.Today;
                p.DateOfNextMenstruation = DateTime.Today.AddDays(p.DurationСycle);
                p.DurationMenstruationVariable = -1;
                p.isNotify = true;
                new DataBaseControllerBase<Period>(new RemPerContext()).Update(p);
                botControllerBase.PrintMessage($"Цикл закінчився.\nНаступний період: {p.DateOfNextMenstruation}.", p.ChatId);
                p.NotifyTheUser.ForEach(n =>
                {
                    botControllerBase.PrintMessage($"Цикл закінчився.\nНаступний період: {p.DateOfNextMenstruation.ToShortDateString()}.", n.ChatId);
                });
            });
        }
    }
}
