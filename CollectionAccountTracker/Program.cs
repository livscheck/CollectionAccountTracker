using CollectionAccountTracker.Components;
using CollectionAccountTracker.Data;
using CollectionAccountTracker.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// API controllers in Controllers/
builder.Services.AddControllers();

// Connection string comes from user secrets in Development (ConnectionStrings:Default)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// The pages call the API in this same app, so the HttpClient's base address is the app's own URL.
// Scoped: one per user connection, created when a page first asks for AccountAPIClient.
builder.Services.AddScoped(sp =>
{
    var navigation = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(navigation.BaseUri) };
});
builder.Services.AddScoped<AccountAPIClient>();

var app = builder.Build();

// Create the database if it doesn't exist and fill it with fake data the first time.
// EnsureCreated won't update an existing database: after changing a model, reset with
// `docker compose down -v` then `docker compose up -d`.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Initialize(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();

// No home page: the app opens on Accounts
app.MapGet("/", () => Results.Redirect("/accounts"));

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
