@echo off
echo Stopping any existing SqlMcpServer processes...
taskkill /F /IM SqlMcpServer.exe >nul 2>&1

echo Cleaning and building project...
dotnet clean SqlMcpServer.csproj >nul
dotnet build SqlMcpServer.csproj

if %ERRORLEVEL% NEQ 0 (
    echo Build failed! Please check for compilation errors.
    pause
    exit /b 1
)

echo Starting SQL MCP Server...
echo Available tools: GetActors, SearchActors, GetActorById, ExecuteCustomQuery
echo Press Ctrl+C to stop the server
echo.
dotnet bin\Debug\net8.0\SqlMcpServer.dll
