using LinkShortener.Api.Application;
using LinkShortener.Api.Data;
using LinkShortener.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;

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
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
var app = builder.Build();

app.UseExceptionHandler(handler => handler.Run(async context =>
{
    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var status = error is BadHttpRequestException ? 400 : 500;
    var detail = status == 400 ? "A requisição contém dados inválidos." : "Ocorreu um erro interno.";
    await Results.Problem(statusCode: status, detail: detail).ExecuteAsync(context);
}));

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<LinkDbContext>().Database.MigrateAsync();
}

app.MapLinkEndpoints();
app.Run();

public partial class Program;
