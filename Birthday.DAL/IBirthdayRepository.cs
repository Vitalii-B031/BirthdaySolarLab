namespace Birthday.DAL;

public interface IBirthdayRepository
{
    BirthdayPerson[] GetAll();
    BirthdayPerson GetById(int id);
    BirthdayPerson Add(BirthdayPerson birthdayPerson);
    void Update(BirthdayPerson birthdayPerson);
    void Delete(int id);
    BirthdayPerson? GetByTelegramUserName(string telegramUsername);
}

