using MySuperUniversalBot_BL.Controller;
using RemBerBot_BL.Controller.Controller;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var botClient = new TelegramBotClient("5268015233:AAFtYMakBaqz-SvLgrmN14IByvkLTP2-404");
using var cts = new CancellationTokenSource();
await botClient.DeleteWebhookAsync();

string messageText = "123123";
long chatId = 0;


BotController botController = new();
BotControllerBase botControllerBase = new();

ReminderController reminderController = new();
PeriodController periodController = new();
TutorialController tutorialController = new();

Dictionary<long, string> UserTutorialDictionary = new();

reminderController.CheckReminders();
periodController.CheckPeriodThread();
periodController.StartMenstruationThread();
periodController.ChangeIsNotifyThread();


var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = { }
};


botClient.StartReceiving(
    HandleUpdateAsync,
    HandleErrorAsync,
    receiverOptions,
    cancellationToken: cts.Token);


var me = await botClient.GetMeAsync();


Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();


cts.Cancel();


async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    if (update.Type == UpdateType.CallbackQuery)
    {
        chatId = update.CallbackQuery.Message.Chat.Id;
        messageText = update.CallbackQuery.Data;

        if (messageText.Contains("tutorial"))
        {
            botControllerBase.SetDictionary(chatId, messageText, UserTutorialDictionary);
            await EndOfTutorial(chatId, messageText, messageText, update, cancellationToken);
            Console.WriteLine($"{chatId}: {messageText}");
        }
        else
        {
            botController.CallbackQueryAsync(update.CallbackQuery, update, cancellationToken);
            Console.WriteLine($"{chatId}: {messageText}");
        }
    }
    else if (update?.Message?.Type == MessageType.Text)
    {
        chatId = update.Message.Chat.Id;
        messageText = update.Message.Text;

        if (UserTutorialDictionary.ContainsKey(chatId))
        {
            if (await EndOfTutorial(chatId, messageText, UserTutorialDictionary[chatId], update, cancellationToken))
            {
                Console.WriteLine($"{chatId}: {messageText}");
                return;
            }
        }

        await botController.CheckAnswerAsync(messageText, chatId, update, cts.Token);
        Console.WriteLine($"{chatId}: {messageText}");
    }
}

async Task<bool> EndOfTutorial(long chatId, string messagetText, string tutorialUser, Update update, CancellationToken cancellationToken)
{
    bool isOk = false;

    if (await tutorialController.CheckTutorial(chatId, messageText, tutorialUser, update, cancellationToken))
    {
        UserTutorialDictionary.Remove(chatId);
        isOk = true;
    }
    else
    {
        isOk = true;
    }

    return isOk;
}

Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    botController.PrintMessage("Я закантачився.", 501103243);
    return Task.CompletedTask;
}

