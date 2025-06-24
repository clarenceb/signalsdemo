
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Register graceful shutdown logic
var lifetime = app.Lifetime;
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation("SIGTERM received. Starting graceful shutdown...");

    // Simulate cleanup work
    Thread.Sleep(10000); // Replace with real cleanup logic

    logger.LogInformation("Cleanup complete. Exiting.");
});


app.Run();
