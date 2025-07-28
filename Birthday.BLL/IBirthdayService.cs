namespace Birthday.BLL;

public interface IBirthdayService
{
    BirthdayPerson[] GetAll();
    BirthdayPerson GetById(int id);
    void Add(BirthdayPerson birthdayPerson);
}