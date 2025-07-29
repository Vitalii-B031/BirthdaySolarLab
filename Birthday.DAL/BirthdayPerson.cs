namespace Birthday.DAL;

public class BirthdayPerson
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Birthday { get; set; }
    public string TelegramUserName { get; set; } = "";
    public string PhotoPath { get; set; } = "";
    public long? TelegramChatId { get; set; }
}