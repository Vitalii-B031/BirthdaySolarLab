using Birthday.BLL;
using Birthday.DAL;
using Birthday.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using BirthdayPerson = Birthday.BLL.BirthdayPerson;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTransient<IBirthdayService,BirthdayService>();
builder.Services.AddTransient<IBirthdayRepository, BirthdayRepository>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       "Host=localhost;Port=5432;Database=birthdaydb;Username=birthdayuser;Password=birthdaypass";
builder.Services.AddDbContext<BirthdayDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHostedService<TelegramNotificationService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowAll");
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

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "photos")),
    RequestPath = "/photos"
});

app.MapGet("/birthdays/{id}",  (IBirthdayService birthdayService, int id) => birthdayService.GetById(id));
app.MapGet("/birthdays", (IBirthdayService birthdayService) => birthdayService.GetAll());
app.MapPost("/birthdays", (IBirthdayService birthdayService, BirthdayPerson person) => birthdayService.Add(person));
app.MapPut("/birthdays/{id}", (IBirthdayService birthdayService, int id, BirthdayPerson person) =>
{
    person.Id = id;
    birthdayService.Update(person);
});
app.MapDelete("/birthdays/{id}",
    (IBirthdayService birthdayService, int id) => birthdayService.Delete(id));

app.MapPost("/birthdays/{id}/photo", async (int id, IBirthdayService birthdayService, HttpRequest request) =>
{
    var person = birthdayService.GetById(id);
    if (person is null)
    {
        return Results.NotFound();
    }

    if (!request.Form.Files.Any())
    {
        return Results.BadRequest("No file uploaded.");
    }

    var file = request.Form.Files[0];
    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "photos");
    if (!Directory.Exists(uploads))
    {
        Directory.CreateDirectory(uploads);
    }

    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
    var filePath = Path.Combine(uploads, fileName);

    await using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    person.PhotoPath = $"photos/{fileName}";
    birthdayService.Update(person);

    return Results.Ok(new { path = person.PhotoPath });
});

app.Run();

