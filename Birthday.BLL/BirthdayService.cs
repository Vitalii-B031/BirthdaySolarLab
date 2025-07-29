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
        return repository.GetAll().Select(Map).ToArray();
    }

    public BirthdayPerson GetById(int id)
    {
        var person = repository.GetById(id);
        return Map(person);
    }

    public BirthdayPerson Add(BirthdayPerson birthdayPerson)
    {
        return Map(repository.Add(Map(birthdayPerson)));
    }

    public void Update(BirthdayPerson birthdayPerson)
    {
        repository.Update(Map(birthdayPerson));
    }

    public void Delete(int id)
    {
        repository.Delete(id);
    }

    public BirthdayPerson[] GetUpcoming()
    {
        var persons = GetAll();
        var today = DateTime.Now;
        return persons.Where(p => p.Birthday.Month == today.Month && p.Birthday.Day == today.Day).ToArray();
    }

    public BirthdayPerson GetPersonByTelegramUserName(string telegramUsername)
    {
        var person = repository.GetByTelegramUserName(telegramUsername);

        return person == null ? null : Map(person);
    }

    private DAL.BirthdayPerson Map(BLL.BirthdayPerson birthdayPerson)
    {
        return new DAL.BirthdayPerson
        {
            Birthday = birthdayPerson.Birthday,
            Name = birthdayPerson.Name,
            Id = birthdayPerson.Id,
            TelegramUserName = birthdayPerson.TelegramUserName,
            TelegramChatId = birthdayPerson.TelegramChatId,
            PhotoPath = birthdayPerson.PhotoPath,
        };
    }
    
    private BLL.BirthdayPerson Map(DAL.BirthdayPerson birthdayPerson)
    {
        return new BLL.BirthdayPerson
        {
            Birthday = birthdayPerson.Birthday,
            Name = birthdayPerson.Name,
            Id = birthdayPerson.Id,
            TelegramUserName = birthdayPerson.TelegramUserName,
            TelegramChatId = birthdayPerson.TelegramChatId,
            PhotoPath = birthdayPerson.PhotoPath,
        };
    }
}