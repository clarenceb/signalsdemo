
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using System.Threading;

using SignalsDemo.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<ShutdownHub>("/shutdownhub");

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

// Use an async method to handle the ApplicationStopping event
lifetime.ApplicationStopping.Register(() =>
{
    var scope = app.Services.CreateScope();
    var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ShutdownHub>>();

    // Run the async logic in a separate task
    Task.Run(async () =>
    {
        logger.LogInformation("SIGTERM received. Starting graceful shutdown...");

        // Notify clients that the application is shutting down
        await hubContext.Clients.All.SendAsync("Shutdown", "Shutting down");

        // Wait for 10 seconds
        await Task.Delay(10000);

        // Notify clients that shutdown is complete
        await hubContext.Clients.All.SendAsync("ShutdownComplete", "Shutdown complete");

        logger.LogInformation("Cleanup complete. Exiting.");
    }).Wait(); // Ensure the task completes before exiting
});

await app.RunAsync();
