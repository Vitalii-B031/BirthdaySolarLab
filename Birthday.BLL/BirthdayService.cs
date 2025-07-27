using Birthday.DAL;

namespace Birthday.BLL;

public class BirthdayService : IBirthdayService
{
    private IBirthdayRepository repository;

    public BirthdayService(IBirthdayRepository repository)
    {
        this.repository = repository;
    }
    public BirthdayPerson[] GetAll()
    {
        return repository.GetAll().Select(bp => new BirthdayPerson
        {
            Birthday = bp.Birthday,
            Name = bp.Name,
            Id = bp.Id,
            TelegramUserName = bp.TelegramUserName,
            TelegramChatId = bp.TelegramChatId,
            PhotoPuth = bp.PhotoPuth,
        }).ToArray();
    }
}