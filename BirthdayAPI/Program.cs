using Birthday.BLL;
using Birthday.DAL;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTransient<IBirthdayService,BirthdayService>();
var connectionString = "Host=localhost;Port=5432;Database=birthdaydb;Username=birthdayuser;Password=birthdaypass";
builder.Services.AddDbContext<BirthdayDbContext>(options => options.UseNpgsql(connectionString));
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


app.MapGet("/test", (IBirthdayService birthdayService) => birthdayService.GetAll());
app.Run();

