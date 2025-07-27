namespace Birthday.BLL;

public class BirthdayPerson
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime Birthday { get; set; }
    public string TelegramUserName { get; set; } = "";
    public string PhotoPuth { get; set; } = "";
    public long? TelegramChatId { get; set; }
}