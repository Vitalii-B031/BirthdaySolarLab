using System.Text.Json;
using System.Text.Json.Serialization;
using Cronos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Birthday.Telegram;

public class TelegramNotificationService : BackgroundService
{
    private IServiceScopeFactory _serviceScopeFactory;
    private IConfiguration _configuration;
    private HttpClient _httpClient = new HttpClient();
    private int lastUpdateId = 0;

    public TelegramNotificationService(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _configuration = configuration;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Telegram Notification Service is running.");
        var botToken = _configuration["TelegramBot:Token"];
        var cronSchedule = _configuration["NotificationService:CronSchedule"];
        if (string.IsNullOrEmpty(cronSchedule) || string.IsNullOrEmpty(botToken))
        {
            Console.WriteLine("Missing configuration TelegramBot:Token or TelegramBot:CronSchedule");
            return;
        }
        var expression = CronExpression.Parse(cronSchedule);
        while (!stoppingToken.IsCancellationRequested)
        {
            await GetChatIds(botToken,stoppingToken);
        }
    }

    private async Task GetChatIds(string botToken, CancellationToken token)
    {
        try
        {
            var getUpdatesUrl = $"https://api.telegram.org/bot{botToken}/getUpdates?offset={lastUpdateId + 1}";
            Console.WriteLine($"Polling Telegram for updates with offset: {lastUpdateId + 1}");
            var response = await _httpClient.GetAsync(getUpdatesUrl, token);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(token);
            var updates = JsonSerializer.Deserialize<TelegramUpdatesResponse>(responseContent);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error polling for new chat IDs: {e.Message}");
            throw;
        }
    }
}
public class TelegramUpdatesResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("result")]
    public List<TelegramUpdate>? Result { get; set; }
}

public class TelegramUpdate
{
    [JsonPropertyName("update_id")]
    public long UpdateId { get; set; }

    [JsonPropertyName("message")]
    public TelegramMessage? Message { get; set; }
}

public class TelegramMessage
{
    [JsonPropertyName("message_id")]
    public long MessageId { get; set; }

    [JsonPropertyName("from")]
    public TelegramUser? From { get; set; }

    [JsonPropertyName("chat")]
    public TelegramChat? Chat { get; set; }

    [JsonPropertyName("date")]
    public long Date { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }
}

public class TelegramUser
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }
}

public class TelegramChat
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }
}