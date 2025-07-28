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
    
}