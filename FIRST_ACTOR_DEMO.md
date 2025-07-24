# SQL MCP Server - First Actor Query Demonstration

## How to get the first actor using the MCP server

When you ask: **"Show me the first actor in SQL"**

The MCP server will respond using one of these tools:

### Method 1: Using GetActors tool
**MCP Query**: "Find the first actor"
**Tool Used**: `GetActors`
**Parameters**: `limit=1, offset=0`
**SQL Equivalent**: `SELECT TOP 1 * FROM actor ORDER BY actor_id`

**Expected JSON Response**:
```json
{
  "actors": [
    {
      "actorId": 1,
      "firstName": "John",
      "lastName": "Doe", 
      "lastUpdate": "2025-07-23T10:30:00"
    }
  ],
  "count": 1,
  "offset": 0,
  "limit": 1,
  "total": 25
}
```

### Method 2: Using GetActorById tool
**MCP Query**: "Get actor with ID 1"
**Tool Used**: `GetActorById`
**Parameters**: `actorId=1`
**SQL Equivalent**: `SELECT * FROM actor WHERE actor_id = 1`

**Expected JSON Response**:
```json
{
  "actorId": 1,
  "firstName": "John",
  "lastName": "Doe",
  "lastUpdate": "2025-07-23T10:30:00"
}
```

### Method 3: Using ExecuteCustomQuery tool
**MCP Query**: "Execute: SELECT TOP 1 * FROM actor ORDER BY actor_id"
**Tool Used**: `ExecuteCustomQuery`
**Parameters**: `sqlQuery="SELECT TOP 1 * FROM actor ORDER BY actor_id"`

**Expected JSON Response**:
```json
{
  "query": "SELECT TOP 1 * FROM actor ORDER BY actor_id",
  "results": [
    {
      "actorId": 1,
      "firstName": "John",
      "lastName": "Doe",
      "lastUpdate": "2025-07-23T10:30:00"
    }
  ],
  "count": 1
}
```

## Setting up the database

To test this, you need to:

1. **Ensure your database has data**:
   ```sql
   -- Run this in SQL Server Management Studio or sqlcmd
   USE F0002;
   
   -- Check if actor table exists and has data
   SELECT TOP 1 * FROM actor ORDER BY actor_id;
   ```

2. **If no data exists**, run the sample-data.sql script:
   ```bash
   sqlcmd -S DESKTOP-5PD7SLV\SQLEXPRESS -d F0002 -i sample-data.sql
   ```

## Running the MCP Server

1. **Start the server**:
   ```bash
   dotnet run --project SqlMcpServer.csproj
   ```

2. **Configure your MCP client** with this configuration:
   ```json
   {
     "mcpServers": {
       "sql-mcp-server": {
         "command": "dotnet",
         "args": ["run", "--project", "c:\\zips\\vscodeprojs\\App6\\SqlMcpServer.csproj"]
       }
     }
   }
   ```

3. **Ask your MCP client**: "Show me the first actor from the database"

The AI assistant will automatically choose the most appropriate tool and return the first actor's information in a structured JSON format.
