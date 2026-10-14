using LinkShortener.Api.Application;
using LinkShortener.Api.Data;
using LinkShortener.Api.Endpoints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<LinkDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Links") ?? "Data Source=links.db"));
builder.Services.AddScoped<ILinkRepository, EfLinkRepository>();
builder.Services.AddSingleton<ICodeRandom, CryptoCodeRandom>();
builder.Services.AddSingleton<CodeGenerator>();
builder.Services.AddSingleton<IHostAddressResolver, DnsHostAddressResolver>();
builder.Services.AddSingleton<TargetUrlValidator>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<LinkService>();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<LinkDbContext>().Database.MigrateAsync();
}

app.MapLinkEndpoints();
app.Run();

public partial class Program;
