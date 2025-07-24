using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using SqlMcpServer.Data;
using SqlMcpServer.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to use stderr to avoid interfering with MCP communication on stdout
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Get connection string from configuration, with fallback to safe default
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=localhost\\SQLEXPRESS;Integrated Security=SSPI;Initial Catalog=YourDatabase;Encrypt=false;app=SqlMcpServer;Connection Timeout=30;Command Timeout=30";

// Configure services
builder.Services.AddDbContext<SqlMcpDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

builder.Services.AddScoped<DatabaseTools>();

// Configure MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

try
{
    var app = builder.Build();
    
    // Test database connection
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<SqlMcpDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            await context.Database.CanConnectAsync();
            logger.LogInformation("Database connection successful!");
        }
        catch (Exception dbEx)
        {
            logger.LogError("Database connection failed: {Message}", dbEx.Message);
            logger.LogInformation("Server will start but database operations may fail");
        }
    }

    // Run the MCP server
    Console.Error.WriteLine("SQL MCP Server starting...");
    Console.Error.WriteLine("Available tools: GetActors, SearchActors, GetActorById, ExecuteCustomQuery");
    await app.RunAsync();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error starting MCP server: {ex.Message}");
    Environment.Exit(1);
}
