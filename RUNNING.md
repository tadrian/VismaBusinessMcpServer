# Running Instructions for SQL MCP Server

## Quick Start

1. **Prerequisites Check**:
   - Ensure .NET 8.0 SDK is installed: `dotnet --version`
   - Verify SQL Server Express is running on `DESKTOP-5PD7SLV\SQLEXPRESS`
   - Confirm database `F0002` exists and has an `actor` table

2. **Build the Project**:
   ```bash
   dotnet build SqlMcpServer.csproj
   ```

3. **Run the MCP Server**:
   ```bash
   dotnet run --project SqlMcpServer.csproj
   ```

## MCP Client Configuration

### For GitHub Copilot or Claude Desktop

Create or update your MCP configuration file with:

```json
{
  "mcpServers": {
    "sql-mcp-server": {
      "command": "dotnet",
      "args": ["run", "--project", "c:\\zips\\vscodeprojs\\App6\\SqlMcpServer.csproj"],
      "env": {}
    }
  }
}
```

### Configuration File Locations:

- **Claude Desktop (Windows)**: `%APPDATA%\Claude\claude_desktop_config.json`
- **Claude Desktop (macOS)**: `~/Library/Application Support/Claude/claude_desktop_config.json`
- **Other MCP Clients**: Check your client's documentation for config file location

## Testing the Server

Once connected to an MCP client, you can test with these queries:

1. **"Find the first 10 actors"** - Uses `GetActors` tool
2. **"Search for actors named Smith"** - Uses `SearchActors` tool  
3. **"Get actor with ID 5"** - Uses `GetActorById` tool
4. **"Execute: SELECT TOP 5 * FROM actor"** - Uses `ExecuteCustomQuery` tool

## Database Schema Requirements

Your `actor` table should have this structure:

```sql
CREATE TABLE actor (
    actor_id INT IDENTITY(1,1) PRIMARY KEY,
    first_name NVARCHAR(45) NOT NULL,
    last_name NVARCHAR(45) NOT NULL,
    last_update DATETIME NOT NULL DEFAULT GETDATE()
);
```

## Troubleshooting

- **Connection Issues**: Check the connection string in `Program.cs`
- **Database Access**: Ensure Windows Authentication is properly configured
- **Server Not Responding**: Check console output for error messages
- **MCP Client Issues**: Verify the file path in your MCP configuration is correct
