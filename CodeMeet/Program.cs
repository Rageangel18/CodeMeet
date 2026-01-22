using System.Net.Http;
using CodeMeet.BLL;
using CodeMeet.DAL;
using CodeMeet.Endpoints;
using CodeMeet.Hubs;
using CodeMeet.Auth;
using CodeMeet.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using CodeMeet.Messaging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using CodeMeet.Hubs.Editor;
using CodeMeet.BLL.Services.Interfaces;
using CodeMeet.Web.Security;

static string SanitizeConnectionString(string? cs)
{
    if (string.IsNullOrWhiteSpace(cs))
        return "<empty>";

    try
    {
        var parts = cs.Split(';', StringSplitOptions.RemoveEmptyEntries);
        var safeParts = parts
            .Where(p => !p.TrimStart().StartsWith("Password=", StringComparison.OrdinalIgnoreCase) &&
                        !p.TrimStart().StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase));
        return string.Join(";", safeParts);
    }
    catch
    {
        return "<failed to parse>";
    }
}

var builder = WebApplication.CreateBuilder(args);

// ---------- DB ----------


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    DotNetEnv.Env.Load();
    connectionString = DotNetEnv.Env.GetString("default_connection");
}

if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString =
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection") ??
        Environment.GetEnvironmentVariable("CUSTOMCONNSTR_DefaultConnection") ??
        Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
}




if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. " +
        "Проверь: env 'ConnectionStrings__DefaultConnection' или Azure Connection Strings.");
}

Console.WriteLine("[Startup] DefaultConnection = " + SanitizeConnectionString(connectionString));

builder.Services.AddDbContext<CodeMeetDbContext>(options =>
{
    options.UseNpgsql(
        connectionString,
        b => b.MigrationsAssembly("CodeMeet.DAL"));
});

// ---------- BLL ----------
builder.Services.AddCodeMeetBll();

// ---------- AUTH (единая для API + Blazor) ----------
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "codemeet_auth";
        options.SlidingExpiration = true;
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
    });

builder.Services.AddAuthorization();


builder.Services.AddScoped<IUserRoleResolver, CookieUserRoleResolver>();

// ---------- Blazor ----------
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

builder.Services.AddScoped<AuthModalState>();

// ---------- SignalR ----------
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 1024 * 1024;
});

// ---------- Swagger (DEV ONLY) ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CodeMeet API",
        Version = "v1",
        Description = "API documentation for CodeMeet interview platform"
    });
});


//---------- RabbitMQ Messaging ----------
builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection("Rabbit"))
    .ValidateOnStart();
builder.Services.AddSingleton(sp => sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value);
builder.Services.AddSingleton(sp =>
{
    var opt = sp.GetRequiredService<RabbitMqOptions>();
    return RabbitMqConnectionFactory.Create(opt);
});

builder.Services.AddSingleton<IExecRunPublisher, CodeMeet.Messaging.ExecRunPublisher>();

builder.Services.AddHostedService<ExecCompletedConsumer>();

builder.Services.AddSingleton<IEditorStateStore, InMemoryEditorStateStore>();


builder.Services.AddSingleton(sp =>
{
    var opt = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<RabbitMqOptions>>().Value;
    var log = sp.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>().CreateLogger("Rabbit");
    var rabbitoptions = new RabbitMqOptions();
    log.LogInformation("Rabbit config: Host={Host} Port={Port} User={User} RunQueue={Run} CompletedQueue={Completed}",
        opt.Host, opt.Port, opt.User, opt.QueueRun, opt.QueueCompleted);
    return RabbitMqConnectionFactory.Create(opt);
});


var app = builder.Build();

var ropt = app.Services.GetRequiredService<RabbitMqOptions>();
app.Logger.LogInformation("Rabbit: {Host}:{Port} user={User} run={Run} completed={Completed}",
    ropt.Host, ropt.Port, ropt.User, ropt.QueueRun, ropt.QueueCompleted);
// ---------- DB migrations at startup ----------

using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<CodeMeetDbContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("[Startup] DB migrate failed: " + ex);
    }
}

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CodeMeet API v1");
    options.RoutePrefix = "swagger";
});

app.UseExceptionHandler("/Error");
app.UseHsts();


// ---------- Pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapAdminOrganizationEndpoints();
app.MapAdminUserEndpoints();
app.MapSessionsEndpoints();
app.MapQuestionsEndpoints();
app.MapParticipantsEndpoints();

app.MapHub<SessionHub>("/hubs/session");
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");




app.MapGet("/health/rabbit", async (RabbitMqOptions opt) =>
{
    try
    {
        var conn = RabbitMqConnectionFactory.Create(opt);
        var ch = await conn.CreateChannelAsync();
        await ch.QueueDeclareAsync(opt.QueueRun, durable: true, exclusive: false, autoDelete: false);
        await ch.QueueDeclareAsync(opt.QueueCompleted, durable: true, exclusive: false, autoDelete: false);
        await ch.CloseAsync();
        await conn.CloseAsync();
        return Results.Ok(new { rabbit = "ok" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();
