# MCP Configuration Examples for SQL MCP Server

## For Claude Desktop (Windows)
**File location**: `%APPDATA%\Claude\claude_desktop_config.json`

```json
{
  "mcpServers": {
    "sql-mcp-server": {
      "command": "dotnet",
      "args": ["c:\\zips\\vscodeprojs\\App6\\bin\\Debug\\net8.0\\SqlMcpServer.dll"],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      },
      "cwd": "c:\\zips\\vscodeprojs\\App6"
    }
  }
}
```

## For Claude Desktop (Alternative - using batch script)
```json
{
  "mcpServers": {
    "sql-mcp-server": {
      "command": "c:\\zips\\vscodeprojs\\App6\\start-server.bat",
      "args": [],
      "env": {}
    }
  }
}
```

## For GitHub Copilot or other MCP clients
```json
{
  "mcpServers": {
    "sql-mcp-server": {
      "command": "dotnet",
      "args": ["c:\\zips\\vscodeprojs\\App6\\bin\\Debug\\net8.0\\SqlMcpServer.dll"],
      "env": {
        "DOTNET_ENVIRONMENT": "Production"
      },
      "cwd": "c:\\zips\\vscodeprojs\\App6"
    }
  }
}
```

## Important Notes:

### ✅ **Why This Configuration Works Better:**
- **No Build Conflicts**: Uses pre-compiled DLL instead of `dotnet run`
- **Faster Startup**: No compilation needed during MCP client connection
- **More Stable**: Avoids file locking issues during runtime
- **Production Ready**: Uses compiled binary for better performance

### ⚠️ **Before Using:**
1. **Build the project first**:
   ```bash
   dotnet build SqlMcpServer.csproj
   ```

2. **Verify the DLL exists**:
   ```
   c:\zips\vscodeprojs\App6\bin\Debug\net8.0\SqlMcpServer.dll
   ```

3. **Test manually** (optional):
   ```bash
   dotnet c:\zips\vscodeprojs\App6\bin\Debug\net8.0\SqlMcpServer.dll
   ```

### 🔧 **Alternative: Use the Batch Script**
For easier management, you can use the `start-server.bat` script which:
- Kills any existing processes
- Cleans and rebuilds the project
- Starts the server fresh

### 🎯 **Available Database Tools:**
- `GetActors` - Get actors with pagination
- `SearchActors` - Search actors by name  
- `GetActorById` - Get specific actor by ID
- `ExecuteCustomQuery` - Run custom SELECT statements

### 💡 **Example Queries to Try:**
- "Show me the first 10 actors"
- "Find actors with the name Smith"
- "Get actor with ID 5"
- "Execute: SELECT TOP 5 * FROM actor WHERE first_name LIKE 'A%'"
