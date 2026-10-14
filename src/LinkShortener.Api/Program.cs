using LinkShortener.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LinkDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Links") ?? "Data Source=links.db"));
builder.Services.AddScoped<ILinkRepository, EfLinkRepository>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<LinkDbContext>().Database.MigrateAsync();
}

app.Run();
