using Gimzo.AppServices.Models;
using Gimzo.Infrastructure.Database;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
    .AddJsonFile("secrets.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
});

builder.Services.AddControllersWithViews();

builder.Services.AddMemoryCache();

builder.Services.Configure<JsonOptions>(o =>
{
    o.SerializerOptions.IncludeFields = true;
    o.SerializerOptions.PropertyNameCaseInsensitive = true;
});

// IConfiguration
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var dbDefPair = DbDefPair.GetPairsFromConfiguration(builder.Configuration).ToArray();
builder.Services.AddSingleton(x => new UiModelService(dbDefPair[0],x.GetRequiredService<IMemoryCache>(),
    x.GetRequiredService<ILogger<UiModelService>>()));
var app = builder.Build();

app.UseStaticFiles();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
