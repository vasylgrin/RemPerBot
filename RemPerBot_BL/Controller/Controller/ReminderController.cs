using MySuperUniversalBot_BL.Controller.ControllerBase;
using RemBerBot_BL.Controller.Controller;
using RemBerBot_BL.Controller.DataBase;
using RemBerBot_BL.Controller.Interface;
using RemBerBot_BL.Models;
using static MySuperUniversalBot_BL.Controller.BotControllerBase;

namespace MySuperUniversalBot_BL.Controller
{
    public class ReminderController : ObjectControllerBase, IObjectControllerBase
    {
        #region Variable

        Dictionary<long, string> NameBirthdayDictionary = new();
        Dictionary<long, DateTime> BirthDateDictionary = new();
        Dictionary<long, int> ClearDictionaryUser = new();

        BotControllerBase botControllerBase = new();

        #endregion

        public async Task<bool> Add(string inputText, long chatId, CancellationToken cancellationToken)
        {
            bool isOk = false;

            await Task.Run(() =>
            {
                var operationEnum = DictionaryController.OperationDictionary[chatId];
                if (operationEnum == OperationEnum.addBirthDate)
                {
                    botControllerBase.PrintKeyboard("Введи ім'я імениника.\nНаприклад: Папочка.", chatId, botControllerBase.SetupKeyboard(GeneralCommands.Назад.ToString()), cancellationToken);

                    DictionaryController.OperationDictionary[chatId] = OperationEnum.addBirthDateOfUsername;
                    DictionaryController.NavigationDictionary[chatId] = NavigationEnum.addBirthDate;
                }
                else if (operationEnum == OperationEnum.addBirthDateOfUsername)
                {
                    if (!string.IsNullOrWhiteSpace(inputText))
                    {
                        botControllerBase.PrintMessage($"Введи дату народження.\nНаприклад: 01.01.2001", chatId);

                        NameBirthdayDictionary.SetDictionary(chatId, inputText);
                        ClearDictionaryUser.SetDictionary(chatId, 5);

                        DictionaryController.OperationDictionary[chatId] = OperationEnum.addDayOfBirthDate;
                    }
                    else
                        botControllerBase.PrintMessage("Текст не може бути пустим...", chatId);
                }
                else if (operationEnum == OperationEnum.addDayOfBirthDate)
                {
                    if (DateTime.TryParse(inputText, out DateTime dateTime))
                    {
                        BirthDateDictionary.SetDictionary(chatId, dateTime);

                        DictionaryController.OperationDictionary[chatId] = OperationEnum.empty;
                        DictionaryController.NavigationDictionary[chatId] = NavigationEnum.MainMenu;

                        isOk = true;
                    }
                    else
                        botControllerBase.PrintMessage($"Введи коректну дату народження.\nНаприклад: 01.01.2001.", chatId);
                }
            });
            
            return isOk;
        }

        public bool Save(long chatId, out int objectId)
        {
            objectId = 0;

            if (NameBirthdayDictionary.ContainsKey(chatId)
                && BirthDateDictionary.ContainsKey(chatId))
            {
                new DataBaseControllerBase<BirthDate>(new RemPerContext()).Save(new BirthDate(chatId, NameBirthdayDictionary[chatId], BirthDateDictionary[chatId], CalculateAge(chatId)));
                objectId = new DataBaseControllerBase<BirthDate>(new RemPerContext()).Load().Where(x => x.ChatId == chatId).First().Id;

                NameBirthdayDictionary.Remove(chatId);
                BirthDateDictionary.Remove(chatId);
                ClearDictionaryUser.Remove(chatId);

                return true;
            }

            return false;
        }

        public void PrintCurrentObject(long chatId, string callbackName) => botControllerBase.PrintInline($"Ім'я: {NameBirthdayDictionary[chatId]}\nВік: {CalculateAge(chatId)}\nДата народження: {BirthDateDictionary[chatId]}", chatId, botControllerBase.SetupInLine(CallbackQueryCommands.Зберегти.ToString(), callbackName));

        private int CalculateAge(long chatId)
        {
            BirthDateDictionary.TryGetValue(chatId, out DateTime birthDay);

            DateTime now = DateTime.Today;
            int age = now.Year - birthDay.Year;
            if (birthDay > now.AddYears(-age))
                age--;
            return age;
        }

        public async void CheckBirthDate()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    new DataBaseControllerBase<BirthDate>(new RemPerContext()).Load()
                    .Where(birthDay => birthDay.DateOfBirthday == DateTime.Today)
                    .ToList()
                    .ForEach(birthDay =>
                    {
                        botControllerBase.PrintMessage($"Сьогодні день народження в {birthDay.Name}", birthDay.ChatId);
                        new DataBaseControllerBase<BirthDate>(new RemPerContext()).Remove(birthDay);
                    });

                    await Task.Delay(86000000);
                }
            });
        }
    }
}
