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

    public BirthdayPerson GetById(int id)
    {
        var person = repository.GetById(id);
        if (person == null)
        {
            return null;
        }
        return new BirthdayPerson
        {
            Birthday = person.Birthday,
            Name = person.Name,
            Id = person.Id,
            TelegramUserName = person.TelegramUserName,
            TelegramChatId = person.TelegramChatId,
            PhotoPuth = person.PhotoPuth,
        };
    }
}