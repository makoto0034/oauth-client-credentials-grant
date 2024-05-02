using log4net;
using log4net.Config;
using Microsoft.EntityFrameworkCore;
using Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add log4net
builder.Services.AddLog4net();

builder.Services.AddDbContext<DbContext>(options =>
{
    // Configure the context to use an in-memory store.
    options.UseInMemoryDatabase(nameof(DbContext));

    // Register the entity sets needed by OpenIddict.
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()

    // Register the OpenIddict core components.
    .AddCore(options =>
    {
        // Configure OpenIddict to use the EF Core stores/models.
        options.UseEntityFrameworkCore()
            .UseDbContext<DbContext>();
    })

    // Register the OpenIddict server components.
    .AddServer(options =>
    {
        options
            .AllowClientCredentialsFlow();

        options
            .SetTokenEndpointUris("/connect/token");

        // Encryption and signing of tokens
        options
            .AddEphemeralEncryptionKey()
            .DisableAccessTokenEncryption()
            .AddEphemeralSigningKey();

        // Register scopes (permissions)
        options.RegisterScopes("api");

        // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough();
    });

builder.Services.AddHostedService<ClientData>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

public static class Log4netExtensions
{
    public static void AddLog4net(this IServiceCollection services)
    {
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
        services.AddSingleton(LogManager.GetLogger(typeof(Program)));
    }
}