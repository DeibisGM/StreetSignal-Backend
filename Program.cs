using Microsoft.AspNetCore.Identity;
using StreetSignalApi.Configuration;
using StreetSignalApi.Data;
using StreetSignalApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddStreetSignal(builder.Configuration);

var app = builder.Build();

// Global exception handler MUST be first
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "StreetSignal API v1");
        c.RoutePrefix = "swagger";
    });
}

// HTTPS redirection breaks the in-memory TestServer used by WebApplicationFactory<Program>
if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

// Serve uploaded files from wwwroot/uploads via /uploads URL
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed in development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
    await DbSeeder.SeedAsync(db, hasher);
}

app.Run();

// Required so the integration tests' WebApplicationFactory<Program> can pick up this entry point.
public partial class Program { }
