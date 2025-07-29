using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Birthday.BLL;
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
    private long lastUpdateId = 0;

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
        
        using var scope = _serviceScopeFactory.CreateScope();
        var birthdayService = scope.ServiceProvider.GetRequiredService<IBirthdayService>();
        
        while (!stoppingToken.IsCancellationRequested)
        {
            await GetChatIds(botToken,stoppingToken, birthdayService);
            var nextOccurence = expression.GetNextOccurrence(DateTime.UtcNow, TimeZoneInfo.Utc);
            if (nextOccurence.HasValue)
            {
                var delay = nextOccurence.Value - DateTimeOffset.UtcNow;
                Console.WriteLine($"Next birthday notification scheduled for: {nextOccurence.Value} (Delay: {delay.TotalSeconds} seconds)");
                if (delay.TotalMilliseconds > 0)
                {
                    var pollingDelay = TimeSpan.FromSeconds(30);
                    if (delay < pollingDelay)
                    {
                        await Task.Delay(delay, stoppingToken);
                    }
                    else
                    {
                        await Task.Delay(pollingDelay, stoppingToken);
                    }
                }

                var today = DateTime.UtcNow.Date;
                Console.WriteLine($"Checking for birthdays on: {today.ToShortDateString()}");
                var birthdays = birthdayService.GetUpcoming();

                if (birthdays.Any())
                {
                    Console.WriteLine($"Found {birthdays.Length} birthdays today.");
                    foreach (var person in birthdays)
                    {
                        var message = $"Сегодня день рождения у {person.Name}!\nTelegram: {person.TelegramUserName}";
                        var url = $"https://api.telegram.org/bot{botToken}/sendMessage";
                        var payload = new { chat_id = person.TelegramChatId, text = message };
                        Console.WriteLine($"Attempting to send birthday message to {person.TelegramUserName} (Chat ID: {person.TelegramChatId}). Payload: {System.Text.Json.JsonSerializer.Serialize(payload)}");
                        try
                        {
                            var response = await _httpClient.PostAsJsonAsync(url, payload, stoppingToken);
                            var responseContent = await response.Content.ReadAsStringAsync(stoppingToken);
                            Console.WriteLine($"Telegram API response for {person.TelegramUserName}: {response.StatusCode} - {responseContent}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending Telegram message to {person.TelegramUserName}: {ex.Message}");
                        }
                    }
                }
            }
        }
    }

    private async Task GetChatIds(string botToken, CancellationToken token, IBirthdayService birthdayService)
    {
        try
        {
            var getUpdatesUrl = $"https://api.telegram.org/bot{botToken}/getUpdates?offset={lastUpdateId + 1}";
            Console.WriteLine($"Polling Telegram for updates with offset: {lastUpdateId + 1}");
            var response = await _httpClient.GetAsync(getUpdatesUrl, token);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync(token);
            var updates = JsonSerializer.Deserialize<TelegramUpdatesResponse>(responseContent);
            
            if (updates == null)
            {
                throw new ApplicationException("Telegram update response could not be deserialized.");
            }
            
            if (updates.Ok && updates.Result != null && updates.Result.Count > 0)
            {
                foreach (var update in updates.Result)
                {
                    lastUpdateId = Math.Max(lastUpdateId, update.UpdateId);
                    
                    if (update.Message?.Chat?.Id == null || string.IsNullOrEmpty(update.Message.From?.Username)) continue;
                    
                    var chatId = update.Message.Chat.Id;
                    var userName = update.Message.From.Username;
                        
                  
                    var person = birthdayService.GetPersonByTelegramUserName(userName);

                    if (person == null)
                    {
                        continue;
                    }

                    if (person.TelegramChatId != chatId)
                    {
                        person.TelegramChatId = chatId;
                        birthdayService.Update(person);
                        Console.WriteLine($"Updated TelegramChatId for {person.Name} to {chatId}");
                        var welcomeMessage = $"Привет, {person.Name}! Я успешно получил твой Chat ID. Теперь ты будешь получать уведомления о днях рождения.";
                        var sendWelcomeUrl = $"https://api.telegram.org/bot{botToken}/sendMessage";
                        var welcomePayload = new { chat_id = chatId, text = welcomeMessage };
                        try
                        {
                            var welcomeResponse = await _httpClient.PostAsJsonAsync(sendWelcomeUrl, welcomePayload, token);
                            var welcomeResponseContent = await welcomeResponse.Content.ReadAsStringAsync(token);
                            Console.WriteLine($"Welcome message sent to {person.TelegramUserName}: {welcomeResponse.StatusCode} - {welcomeResponseContent}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error sending welcome message to {person.TelegramUserName}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No BirthdayPerson found for Telegram username: {person.TelegramUserName}");
                    }
                }
            }
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