using System.Text.Json.Serialization;
using KodingTask.Api.Ai;
using KodingTask.Api.Contracts;
using KodingTask.Api.Features;
using KodingTask.Api.Hosting;
using KodingTask.Api.Limits;
using KodingTask.Conversion;
using KodingTask.Data;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddApplicationVersion(AppContext.BaseDirectory);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddKodingTaskDatabase(builder.Configuration, builder.Environment);
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<KodingTaskDbContext>()
    .SetApplicationName("KodingTask");
builder.Services.AddLoadBalancerForwardedHeaders();
builder.Services.AddRequestRateLimits(builder.Configuration);
builder.Services.AddSingleton<ClientFingerprint>();
builder.Services.AddUsageLimits(builder.Configuration);

builder.Services.AddSingleton(SpellerCatalog.Default);
builder.Services.AddSingleton<CurrencyConverter>();
builder.Services.Configure<PublicSiteOptions>(builder.Configuration.GetSection(PublicSiteOptions.SectionName));

var aiConfigured = builder.Services.AddAiConversion(builder.Configuration);
builder.Services.AddSingleton(new FeatureAvailability(
    Email: false,
    WhatsApp: false,
    Ai: aiConfigured,
    MachineTranslation: false,
    ExternalLogin: false));

builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpForwarder();
    builder.Services.AddHostedService<ViteDevServer>();
}

var app = builder.Build();

app.UseHealthEndpoint();
app.UseForwardedHeaders();
app.UseSecurityHeaders();
if (!app.Environment.IsDevelopment())
    app.UseHsts();

await app.PrepareDatabaseAsync();

app.UseDefaultFiles();
app.MapStaticAssets();
app.UseRouting();
app.UseRateLimiter();

app.MapControllers();
app.MapOpenApi();
app.Map("/api/{**path}", () => Results.Json(ErrorResponse.NotFound, statusCode: StatusCodes.Status404NotFound));

if (app.Environment.IsDevelopment())
    app.MapForwarder("/{**path}", app.Configuration["Development:ViteServerUrl"] ?? "http://localhost:5179");
else
    app.MapFallbackToFile("/index.html");

app.Run();

public partial class Program;
