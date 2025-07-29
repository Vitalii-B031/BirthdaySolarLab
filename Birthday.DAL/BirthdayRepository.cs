using Microsoft.EntityFrameworkCore;

namespace Birthday.DAL;

public class BirthdayRepository : IBirthdayRepository
{
    private BirthdayDbContext dbContext;

    public BirthdayRepository(BirthdayDbContext context)
    {
        dbContext = context;
    }
    public BirthdayPerson[] GetAll()
    {
        return dbContext.BirthdayPersons.ToArray();
    }

    public BirthdayPerson GetById(int id)
    {
        return dbContext.BirthdayPersons.Find(id);
    }

    public void Add(BirthdayPerson birthdayPerson)
    {
        dbContext.BirthdayPersons.Add(birthdayPerson);
        dbContext.SaveChanges();
    }

    public void Update(BirthdayPerson birthdayPerson)
    {
        dbContext.BirthdayPersons.Update(birthdayPerson);
        dbContext.SaveChanges();
    }

    public void Delete(int id)
    {
        var person = dbContext.BirthdayPersons.Find(id);
        if (person == null)
        {
            return;
        }
        dbContext.BirthdayPersons.Remove(person);
        dbContext.SaveChanges();
    }

    public BirthdayPerson GetByTelegramUserName(string telegramUsername)
    {
        var person = dbContext.BirthdayPersons.AsNoTracking()
            .FirstOrDefault(x => x.TelegramUserName == telegramUsername);

        return person ?? null;
    }
}