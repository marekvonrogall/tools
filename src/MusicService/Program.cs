using System.Net;
using MusicService;
using MusicService.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Limits.MaxRequestBodySize = 1024 * 1024 * 100; // 100 MB
    options.Listen(IPAddress.Any, 5001);
});

builder.Services.AddHttpClient<MusicController>();
builder.Services.AddSingleton<MusicLibrary>();

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var controller = scope.ServiceProvider.GetRequiredService<MusicController>();
    await controller.RefreshLibrary();
}

app.Run();
