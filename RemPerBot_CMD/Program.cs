using MySuperUniversalBot_BL.Controller;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

var botClient = new TelegramBotClient("5268015233:AAFtYMakBaqz-SvLgrmN14IByvkLTP2-404");
using var cts = new CancellationTokenSource();
await botClient.DeleteWebhookAsync();

string messageText = "123123";
long chatId = 0;


BotController botController = new();

ReminderController reminderController = new();
PeriodController periodController =new();

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
    if (update.Type != UpdateType.Message)
    {
        if (update.Type == UpdateType.CallbackQuery)
        {
            botController.CallbackQueryAsync(update.CallbackQuery);
            return;
        }
        
        if (update?.Message?.Type != MessageType.Text)
            return;
    }

    chatId = update.Message.Chat.Id;
    messageText = update.Message.Text;

    botController.CheckAnswerAsync(messageText, chatId, cts.Token);
    Console.WriteLine($"{chatId}: {messageText}");
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
    return Task.CompletedTask;
}

