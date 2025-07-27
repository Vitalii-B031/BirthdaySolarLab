using Birthday.BLL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddTransient<IBirthdayService,BirthdayService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/test", (IBirthdayService birthdayService) => birthdayService.GetAll());
app.Run();

