using Birthday.BLL;
using Birthday.DAL;
using Birthday.Telegram;
using Microsoft.EntityFrameworkCore;
using BirthdayPerson = Birthday.BLL.BirthdayPerson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTransient<IBirthdayService,BirthdayService>();
builder.Services.AddTransient<IBirthdayRepository, BirthdayRepository>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       "Host=localhost;Port=5432;Database=birthdaydb;Username=birthdayuser;Password=birthdaypass";
builder.Services.AddDbContext<BirthdayDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHostedService<TelegramNotificationService>();
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BirthdayDbContext>();
    context.Database.Migrate();
}
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/birthdays/{id}",  (IBirthdayService birthdayService, int id) => birthdayService.GetById(id));
app.MapGet("/birthdays", (IBirthdayService birthdayService) => birthdayService.GetAll());
app.MapPost("/birthdays", (IBirthdayService birthdayService, BirthdayPerson person) => birthdayService.Add(person));
app.MapPut("/birthdays", (IBirthdayService birthdayService, BirthdayPerson person) => birthdayService.Update(person));
app.MapDelete("/birthdays/{id}",
    (IBirthdayService birthdayService, int id) => birthdayService.Delete(id));
app.Run();

