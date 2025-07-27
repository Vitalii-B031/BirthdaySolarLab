namespace Birthday.BLL;

public class BirthdayService : IBirthdayService
{
    public BirthdayPerson[] GetAll()
    {
        return Enumerable.Range(1, 3).Select(i => new BirthdayPerson
        {
            Id = i,
            Birthday = DateTime.UtcNow.AddDays(i),
            Name = "Вася"+i,
            PhotoPuth = i.ToString(),
            TelegramUserName = "Вася"+2*i,
            TelegramChatId = i
        }).ToArray();
    }
}