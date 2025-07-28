namespace Birthday.DAL;

public interface IBirthdayRepository
{
    BirthdayPerson[] GetAll();
    BirthdayPerson GetById(int id);
}

