using Microsoft.EntityFrameworkCore;

namespace Birthday.DAL;

public class BirthdayDbContext: DbContext
{
    public DbSet<BirthdayPerson> BirthdayPersons { get; set; }
    public BirthdayDbContext(DbContextOptions<BirthdayDbContext> options) : base(options)
    {
        
    }
}