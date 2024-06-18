using System.Text;

namespace GameAutomation.Core;

public class TelegramNotifier
{
    public string AppName { get; }
    public string BotToken { get; }
    public long ChatId { get; }
    public long ThreadId { get; }

    private const string DelimiterLine = "--------------------";
    private const string TelegramBaseUrl = "https://api.telegram.org/bot";

    private readonly HttpClient _httpClient;
    private readonly StringBuilder _messageBuilder;

    public TelegramNotifier(string appName, string botToken, long chatId, long threadId = -1)
    {
        AppName = appName;
        BotToken = botToken;
        ChatId = chatId;
        ThreadId = threadId;

        _httpClient = new HttpClient();
        _messageBuilder = new StringBuilder();
    }

    public async Task NotifyAsync(string text)
    {
        _messageBuilder.Append("<code>")
                       .Append($"{DelimiterLine}\n")
                       .Append($"<b>{AppName}</b>\n")
                       .Append($"{DelimiterLine}\n")
                       .Append($"{DateTime.Now}\n")
                       .Append($"{DelimiterLine}\n")
                       .Append($"{text}\n")
                       .Append($"{DelimiterLine}\n")
                       .Append("</code>");

        var requestString = $"{TelegramBaseUrl}{BotToken}/sendMessage?chat_id={ChatId}{(ThreadId != -1 ? $"&threadId={ThreadId}" : "")}&text={_messageBuilder}&parse_mode=Html";

        await _httpClient.GetAsync(requestString);

        _messageBuilder.Clear();
    }
}
