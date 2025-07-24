<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# SQL MCP Server Instructions

This is an MCP (Model Context Protocol) server project that provides database querying capabilities for MSSQL databases.

## Key Features
- Connects to MSSQL databases using Entity Framework Core
- Exposes database tables as MCP tools for AI assistants
- Supports parameterized queries for data retrieval
- Uses STDIO transport for MCP communication

## Development Guidelines
- Follow Entity Framework best practices for database access
- Implement proper error handling for database operations
- Use dependency injection for services and DbContext
- Ensure all MCP tools are properly documented with descriptions
- Test database connectivity before exposing tools

You can find more info and examples at https://modelcontextprotocol.io/llms-full.txt
