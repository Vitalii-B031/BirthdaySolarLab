namespace Birthday.BLL;

public interface IBirthdayService
{
    BirthdayPerson[] GetAll();
    BirthdayPerson GetById(int id);
    BirthdayPerson Add(BirthdayPerson birthdayPerson);
    void Update(BirthdayPerson birthdayPerson);
    void Delete(int id);
    BirthdayPerson[] GetUpcoming();
    BirthdayPerson GetPersonByTelegramUserName(string telegramUsername);
}