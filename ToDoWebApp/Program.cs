using ToDoWebApp.Components;
using Supabase;
using ToDoWebApp.Services;
using DotNetEnv;

// Load environment variables from .env file
Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add environment variables to configuration
builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Supabase client
builder.Services.AddSingleton(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    
    // Try to get from configuration first, then fallback to environment variables
    var url = configuration["SUPABASE_URL"] ?? Environment.GetEnvironmentVariable("SUPABASE_URL");
    var key = configuration["SUPABASE_ANON_KEY"] ?? Environment.GetEnvironmentVariable("SUPABASE_ANON_KEY");

    if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(key))
    {
        throw new InvalidOperationException("Supabase URL or Anon Key is not set up yet. Please check your .env file or configuration.");
    }

    return new Supabase.Client(url, key, new Supabase.SupabaseOptions
    {
        AutoConnectRealtime = true
    });
});

// Register services
builder.Services.AddScoped<TodoService>();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();