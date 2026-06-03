using StreetSignalApi.Configuration;
using StreetSignalApi.Data;

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

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();

// Required so the integration tests' WebApplicationFactory<Program> can pick up this entry point.
public partial class Program { }
