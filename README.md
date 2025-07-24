# Visma Business MCP Server

A specialized Model Context Protocol (MCP) server that provides intelligent database querying capabilities for **Visma Business** SQL databases using Entity Framework Core.

## About Visma Business

**Visma Business** is a comprehensive Enterprise Resource Planning (ERP) system widely used by small to medium-sized businesses across the Nordic region and Europe. It provides integrated business management functionality including:

- **Financial Management**: General ledger, accounts payable/receivable, budgeting, and financial reporting
- **Sales & CRM**: Customer relationship management, sales order processing, and quotation management
- **Purchasing & Inventory**: Supplier management, purchase orders, inven### 💡 Pro Tips for Better Results
- **Use appropriate date ranges**: Use realistic date ranges for your database content
- **Include specific parameters**: "Show 25 products with offset 50"
- **Use date formats**: "From 20240101 to 20241231" (YYYYMMDD format)
- **Specify filter criteria**: "Only active accounts" or "Unpaid transactions only"
- **Request pagination**: "Show me the next 20 results"
- **Ask for totals**: "Include transaction counts and totals"
- **Request details**: "Show full details with related data"
- **Use natural language**: The MCP server understands conversational queries
- **Build on results**: "Now show me the details for order 12345"
- **Combine multiple queries**: "Show me the customer and their recent orders"

### 🎯 **Best Practices for Queries**
**Use proper date formats**: YYYYMMDD format (e.g., 20240101 to 20241231)
**Languages**: Supports international business data with English field names
**Standard Account Structure**: Following Swedish accounting standards
**Data Types**: Products, services, memberships, and consulting offerings
**Business Entities**: Comprehensive support for various organization types
- **Project Management**: Project tracking, time registration, and project accounting
- **Payroll & HR**: Employee management, payroll processing, and human resources administration
- **Reporting & Analytics**: Comprehensive business intelligence and financial reporting tools

### Visma Business Database Model

This MCP server is specifically designed to work with the **Visma Business SQL database schema**, which includes:

- **Actor System**: Unified management for all business actors (customers, suppliers, employees, contacts, prospects, partners)
- **Financial Dimensions**: Multi-dimensional accounting with R1-R12 dimension tables for cost centers, projects, departments
- **Order Management**: All order types (sales orders, purchase orders, quotes, service orders, returns) and document processing
- **Product Catalog**: Comprehensive product and service management with pricing matrices
- **General Ledger**: Complete chart of accounts and transaction processing
- **Business Intelligence**: Pre-built analytics and reporting structures

## What This MCP Server Does

This MCP server acts as an **intelligent query interface** for Visma Business databases, allowing you to:

🔍 **Ask Natural Language Questions** about your business data:
- "Show me sales for last quarter"
- "Find all unpaid customer invoices"
- "What are our top-selling products?"
- "Analyze costs by department"

📊 **Access Business Intelligence** through conversational AI:
- Financial reporting and analysis
- Customer and supplier insights
- Product performance metrics
- Cost center and project analysis

🛠️ **Integrate with AI Tools** like GitHub Copilot, Claude, or custom applications:
- Automatic query generation from natural language
- Structured JSON responses for further processing
- Safe, parameterized database access
- Built-in pagination and filtering

### 🔧 **Technical Approach: Direct SQL Database Access**

**Important Note**: This MCP server uses **direct SQL database queries** to access Visma Business data. It does **NOT** use the Visma Business VBS (Visma Business Services) API or WCF (Windows Communication Foundation) services.

**Advantages of Direct SQL Access:**
- **Real-time Performance** - No API overhead or rate limiting
- **Complete Data Access** - Access to all tables and relationships
- **Complex Queries** - Advanced joins and analytics impossible through APIs
- **No Authentication Overhead** - Direct database connection
- **Bulk Operations** - Efficient processing of large datasets
- **Historical Data** - Full access to archived transactions and audit trails

**vs. VBS/WCF API Approach:**
- VBS/WCF APIs are designed for application integration and business logic
- This MCP server focuses on data analysis and business intelligence
- Direct SQL provides deeper insights and faster query performance
- Complementary approach to standard Visma Business API integrations

## Target Users

- **Business Analysts** who want to query Visma Business data using natural language
- **Developers** building AI-powered business applications on top of Visma Business
- **Finance Teams** needing quick access to financial data and reports
- **Management** requiring instant business insights and KPI monitoring
- **System Integrators** connecting Visma Business with modern AI tools

## Real-World Business Value

### 🎯 **Instant Business Intelligence**
Transform complex SQL queries into simple questions:
- *"Generate our Profit & Loss statement for current year"* → Complete financial statement with comparisons
- *"Show me our financial ratios and health indicators"* → Comprehensive ratio analysis with interpretations
- *"Which customers have overdue invoices?"* → Aging analysis with collection risk assessment
- *"What were our top-selling products last quarter?"* → Advanced product performance analysis
- *"Compare our actual vs budget performance"* → Variance analysis with management insights
- *"Analyze project costs by department"* → Multi-dimensional financial reporting

### 📊 **Advanced Analytics Made Simple**
- **🆕 Complete Financial Statements** - Profit & Loss, Balance Sheet, Cash Flow statements
- **🆕 Financial Ratio Analysis** - Liquidity, profitability, leverage ratios with interpretations
- **🆕 Aging & Credit Management** - Customer receivables aging and collection risk analysis
- **🆕 Profitability Intelligence** - Customer and product margin analysis with insights
- **🆕 Budget Variance Reporting** - Actual vs budget analysis with variance explanations
- **Financial KPIs** - Revenue trends, profit margins, cash flow analysis
- **Operational Metrics** - Inventory turnover, order fulfillment rates, customer satisfaction
- **Cost Management** - Department budgets, project profitability, expense analysis
- **Compliance Reporting** - VAT reporting, audit trails, regulatory compliance

### 🔄 **Modern Workflow Integration**
- **AI-Powered Dashboards** - Real-time business intelligence
- **Automated Reporting** - Scheduled insights delivered to stakeholders
- **Decision Support Systems** - Data-driven business recommendations
- **Integration Hubs** - Connect Visma Business with CRM, e-commerce, and other systems

### 💡 **Competitive Advantages**
- **Faster Decision Making** - Instant access to business data
- **Reduced IT Overhead** - No complex report development needed
- **Better Data Utilization** - Unlock insights hidden in Visma Business database
- **Future-Proof Architecture** - Ready for AI and machine learning initiatives

## Architecture: Direct SQL vs. API Access

### 🎯 **This MCP Server Approach: Direct SQL Database Queries**

This MCP server is designed for **direct SQL database access** to provide maximum performance and flexibility for business intelligence and data analysis.

**What This MCP Server Uses:**
- ✅ **Direct SQL Server Connection** via Entity Framework Core
- ✅ **T-SQL Queries** executed directly against Visma Business database
- ✅ **Read-Only Database Access** for safety and performance
- ✅ **Complete Schema Access** to all 400+ Visma Business tables

**What This MCP Server Does NOT Use:**
- ❌ **VBS (Visma Business Services)** - Business logic APIs
- ❌ **WCF (Windows Communication Foundation)** services
- ❌ **Visma Business Application Server** APIs
- ❌ **Business rule validation** layers

### 📊 **Comparison: SQL Access vs. VBS/WCF APIs**

| Aspect | Direct SQL (This MCP Server) | VBS/WCF APIs |
|--------|------------------------------|---------------|
| **Purpose** | Data analysis & business intelligence | Application integration & transactions |
| **Performance** | Fast - Direct database access | Slower - API overhead |
| **Data Access** | Complete - All tables & relationships | Limited - Exposed business objects only |
| **Query Complexity** | Advanced - Complex joins & analytics | Simple - Predefined operations |
| **Real-time** | Yes - Immediate results | Rate limited - API throttling |
| **Security** | Read-only database permissions | Full business logic validation |
| **Use Case** | Reporting, analytics, AI insights | Data entry, business transactions |

### 🔄 **When to Use Each Approach**

**Use This MCP Server (Direct SQL) For:**
- Business intelligence and reporting
- Data analysis and AI-powered insights  
- Complex multi-table queries
- Historical data analysis
- Performance-critical read operations
- Advanced financial dimension analysis

**Use VBS/WCF APIs For:**
- Creating/updating business transactions
- Enforcing business rules and validation
- Application integration with workflows
- Real-time business process automation
- Maintaining data integrity through business logic

### 🤝 **Complementary Architecture**

This MCP server is designed to **complement** (not replace) Visma Business APIs:
- **Analytics Layer**: Use MCP server for insights and reporting
- **Transaction Layer**: Use VBS/WCF APIs for business operations
- **Hybrid Solutions**: Combine both for complete business applications

## Features

- **Direct SQL Database Access**: Uses Entity Framework Core for direct database queries, bypassing VBS/WCF APIs for maximum performance
- **Visma Business Integration**: Specifically designed for Visma Business SQL database schema and business logic
- **Comprehensive Business Entities**: Support for actors (customers/suppliers), products, orders, invoices, and financial dimensions
- **🆕 Complete Financial Statements**: Generate Profit & Loss, Balance Sheet, and Cash Flow statements with period comparisons
- **🆕 Advanced Financial Ratios**: Calculate liquidity, profitability, leverage, and efficiency ratios with interpretations
- **🆕 Aging Reports & Credit Management**: Detailed receivables aging with risk analysis and collection insights
- **🆕 Profitability Analysis**: Customer and product profitability analysis with margin calculations
- **🆕 Budget Variance Analysis**: Compare actual vs budget performance with variance explanations
- **Financial Intelligence**: Access to chart of accounts, general ledger, and multi-dimensional financial reporting
- **Natural Language Queries**: Convert business questions into optimized SQL queries automatically
- **Advanced Analytics**: Built-in support for sales analysis, customer insights, and comprehensive financial reporting
- **Secure Database Access**: Read-only operations with parameterized queries and input validation
- **Pagination & Performance**: Optimized queries with built-in pagination for large datasets
- **JSON API Format**: Structured responses perfect for AI consumption and further processing
- **Multi-Dimensional Analysis**: Support for Visma Business R1-R12 financial dimensions (cost centers, projects, departments)

## Available MCP Tools

This MCP server provides **27 specialized tools** for querying business data across multiple domains:

### 👥 Actor Management Tools
**1. GetActors** - Retrieves actors with pagination
- Parameters: `limit` (max: 100), `offset`
- Example: "Find the first 20 actors"

**2. SearchActors** - Search actors by name
- Parameters: `searchTerm` (required), `limit`
- Example: "Find actors with name 'Smith'"

**3. GetActorById** - Get specific actor details
- Parameters: `actorId` (required)
- Example: "Get actor with ID 123"

### 📦 Product & Inventory Tools
**4. GetProducts** - List products with search and pagination
- Parameters: `searchTerm`, `limit`, `offset`
- Example: "Show products containing 'computer'"

**5. GetProductPricing** - Get pricing for specific product
- Parameters: `productNumber` (required), `customerPriceGroup`
- Example: "Get pricing for product 1001"

**6. GetProductsWithPricing** - All products with pricing information
- Parameters: `customerPriceGroup`, `limit`, `offset`
- Example: "Show all products with prices"

### 📊 Sales & Analytics Tools
**7. GetSalesByYear** - Detailed sales data for specific year
- Parameters: `year` (required), `limit`
- Example: "Show sales for last year"

**8. GetSalesSummaryByYear** - Sales summary with totals and statistics
- Parameters: `year` (required)
- Example: "Get sales summary for last year"

**9. GetRecentSales** - Recent sales transactions
- Parameters: `days` (default: 30), `limit`
- Example: "Show sales from last 60 days"

### 📋 Order Management Tools
**10. GetOrders** - Order data with filtering options
- Parameters: `orderNumber`, `liableActor`, `deliveryActor`, `fromDate`, `toDate`, `status`, `limit`, `offset`
- Example: "Show orders for customer 12345"

**11. GetOrderLines** - Detailed order line items
- Parameters: `orderNumber`, `productNumber`, `r1`, `r2`, `r3`, `r7`, `limit`, `offset`
- Example: "Get order lines for order 987"

**12. GetOrdersWithFullDetails** - Comprehensive order information with relationships
- Parameters: `orderNumber`, `liableActor`, `deliveryActor`, `fromDate`, `toDate`, `includeFinancialDimensions`, `limit`, `offset`
- Example: "Show detailed order information with actor details"

### 📄 Invoice & Document Tools
**13. GetInvoiceDocuments** - Invoice document management
- Parameters: `orderNumber`, `liableActor`, `deliveryActor`, `fromDate`, `toDate`, `limit`, `offset`
- Example: "Show invoices from last quarter"

### 💰 Financial Transaction Tools
**14. GetCustomerTransactions** - Customer transaction history
- Parameters: `customerNumber`, `transactionType`, `fromDate`, `toDate`, `unpaidOnly`, `limit`, `offset`
- Example: "Show unpaid customer invoices"

**15. GetSupplierTransactions** - Supplier transaction management
- Parameters: `supplierNumber`, `transactionType`, `fromDate`, `toDate`, `unpaidOnly`, `limit`, `offset`
- Example: "List supplier transactions from last month"

### 🏦 Accounting Tools
**16. GetAccounts** - Chart of accounts with filtering
- Parameters: `searchTerm`, `accountType`, `activeOnly`, `limit`, `offset`
- Example: "List all revenue accounts"

**17. GetAccountTransactions** - General ledger transactions
- Parameters: `accountNumber`, `transactionType`, `fromDate`, `toDate`, `limit`, `offset`
- Example: "Show transactions for account 3000"

### 📊 Financial Dimension Tools
**18. GetFinancialDimensions** - Financial dimension master data
- Parameters: `dimensionTable` (R1-R12), `searchTerm`, `limit`, `offset`
- Example: "Show cost centers from R1 dimension"

**19. GetFinancialDimensionAnalysis** - Dimension analysis with totals
- Parameters: `dimensionLevel` (R1-R12), `fromDate`, `toDate`, `limit`
- Example: "Analyze R2 dimension usage"

**20. AnalyzeDimensionUsage** - Detailed dimension usage analysis
- Parameters: `dimensionLevel` (R1-R12), `limit`
- Example: "Show which R3 dimensions are actively used"

### 🔧 Advanced Tools
**21. ExecuteCustomQuery** - Execute custom SQL SELECT statements
- Parameters: `sqlQuery` (required)
- Example: "Execute: SELECT TOP 5 * FROM actor WHERE first_name LIKE 'A%'"

### 💼 **NEW! Financial Statements & Reports**
**22. GetProfitLossStatement** - Generate Profit & Loss statements
- Parameters: `fromDate`, `toDate`, `includeComparison`
- Example: "Generate P&L for current year with previous year comparison"

**23. GetBalanceSheet** - Generate Balance Sheet reports
- Parameters: `asOfDate`, `includeZeroBalances`
- Example: "Show balance sheet as of year end"

**24. GetCashFlowStatement** - Generate Cash Flow statements
- Parameters: `fromDate`, `toDate`
- Example: "Generate cash flow statement for last quarter"

### 📈 **NEW! Financial Ratios & KPIs**
**25. GetFinancialRatios** - Calculate key financial ratios and KPIs
- Parameters: `asOfDate`, `includeIndustryBenchmarks`
- Example: "Calculate financial ratios with interpretations"

### ⏰ **NEW! Aging & Credit Management**
**26. GetCustomerAgingReport** - Detailed customer receivables aging
- Parameters: `asOfDate`, `includePaidInvoices`, `limit`
- Example: "Show customer aging report with overdue analysis"

### 💹 **NEW! Advanced Analytics**
**27. GetProfitabilityAnalysis** - Profitability analysis by customer/product
- Parameters: `analysisType`, `fromDate`, `toDate`, `dimensionLevel`, `limit`
- Example: "Analyze customer profitability for current year"

**28. GetBudgetVarianceAnalysis** - Budget vs actual variance analysis
- Parameters: `budgetYear`, `actualFromDate`, `actualToDate`, `varianceThreshold`, `limit`
- Example: "Compare current year actual vs budget with significant variances"

### 📅 Date Format Notes
All date parameters use YYYYMMDD format (e.g., 20240101 for January 1, 2024)

### 🔒 Security Features
- All tools use parameterized queries to prevent SQL injection
- Only SELECT operations are allowed for data safety
- Input validation on all parameters
- Proper error handling and logging

## Prerequisites

- **Visma Business Installation**: A working Visma Business system with SQL Server database
- **.NET 8.0 SDK or later**: For running the MCP server application
- **SQL Server Access**: Connection to the Visma Business SQL Server database
- **Database Permissions**: Read access to Visma Business database tables
- **Network Connectivity**: Ability to connect to the Visma Business database server

### Visma Business Database Requirements
- Visma Business database with standard schema (400+ tables including Actor, Ord, OrdLn, PrDcMat, etc.)
- SQL Server 2016 or later (2022 recommended as of 2024/2025)
- Proper database user permissions for read operations

## Technical Architecture & Integration

### SQL Server Technology Stack
**Database Engine Requirements:**
- **SQL Server 2022** ✅ (Current standard, deployed fall 2024)
- **SQL Server 2019/2017/2016** ✅ (Legacy support)
- **Compatibility Level** - Matches SQL Server version
- **Service Packs** - Latest cumulative updates recommended

**Performance Specifications:**
- **Standard/Enterprise Edition** - Production environments
- **Express Edition** - Development only (1GB RAM, 10GB database limits)
- **Memory Optimization** - Leverages SQL Server's in-memory capabilities
- **Indexing Strategy** - Optimized for Visma Business query patterns

### Visma API Ecosystem Integration
This MCP server complements the broader Visma API ecosystem through **direct SQL access**:

**Visma Developer Platform APIs (Complementary Solutions):**
- **Business NXT GraphQL API** - Cloud-based business solutions
- **Control Edge API** - Accounting and invoicing automation
- **Visma.net API** - Complete ERP functionality access
- **eAccounting API** - Small business financial management

**Integration Approach:**
- **Direct SQL Access** - This MCP server queries the database directly (not via VBS/WCF APIs)
- **Hybrid Workflows** - Combine direct SQL analytics with API-based operations
- **Performance Focus** - SQL access provides faster queries for business intelligence
- **Complementary Architecture** - Use APIs for transactions, SQL for analytics

**Integration Capabilities:**
- **OData Support** - Compatible with OData connectors and SSIS
- **Real-time Queries** - Immediate database access without API rate limits
- **Bulk Operations** - Efficient large dataset processing via direct SQL
- **Deep Analytics** - Access to complete database schema beyond API limitations

### AI and Modern Integration Features
**Model Context Protocol (MCP) Advantages:**
- **Natural Language Processing** - Convert business questions to SQL
- **Context Awareness** - Understands Visma Business relationships and workflows  
- **AI Tool Integration** - Works with GitHub Copilot, Claude, and custom AI applications
- **Intelligent Caching** - Optimizes repeated query patterns

**Data Access Patterns:**
- **Read-Only Operations** - Ensures data integrity and compliance
- **Parameterized Queries** - SQL injection protection
- **Entity Framework Core** - Modern .NET data access with LINQ support
- **JSON Output** - Structured responses for AI consumption

## Visma Business Compatibility

### Supported Visma Business Versions
- **Visma Business 2020** and later versions (from official Visma documentation)
- **Visma Business Cloud** deployments
- **On-premise installations** with SQL Server database
- **Multi-company environments** (specify database/company in connection string)

### SQL Server Compatibility (Official Visma Requirements)
**Currently Supported (as of 2024/2025):**
- **SQL Server 2022** ✅ (Latest - rolled out in fall 2024)
- **SQL Server 2019** ✅ (all editions and latest cumulative updates)
- **SQL Server 2017** ✅ (all editions and latest cumulative updates) 
- **SQL Server 2016** ✅ (all editions and latest service packs)

**Operating System Requirements:**
- Windows 10/11 (desktop environments)
- Windows Server 2016/2019/2022 (server environments)

**Edition Recommendations:**
- **SQL Server Standard or Enterprise** - Recommended for production
- **SQL Server Express** - Test/development only (limitations: 1GB RAM, 10GB max database size)

### Database Schema Compatibility
This MCP server is designed to work with the standard Visma Business database schema. It automatically adapts to:
- Different Visma Business versions with schema variations
- Custom field additions and modifications
- Multi-language installations
- Various localization configurations (Nordic countries, etc.)

### Performance Considerations
- Optimized for Visma Business database patterns and indexing
- Respects Visma Business performance best practices
- Includes appropriate filtering to handle large production datasets
- Uses Visma Business recommended query patterns

### Integration Capabilities
**Direct SQL Database Access:**
- Direct database queries via Entity Framework Core (not VBS/WCF APIs)
- Read-only operations for data safety
- Parameterized queries for security
- Full access to Visma Business database schema

**API Integration Potential:**
- Compatible with Visma Developer ecosystem (complementary to APIs)
- Can work alongside Visma.net API access for hybrid solutions
- Supports OData and REST integrations for complete business workflows
- Works with Business NXT GraphQL API for cloud-based scenarios

**Note**: This MCP server uses direct SQL access rather than Visma Business VBS/WCF services, providing faster performance and deeper data access for analytics and reporting purposes.

## Installation

1. **Clone or download this project**
2. **Install dependencies**:
   ```bash
   dotnet restore
   ```

3. **Configure database connection**:
   
   **Option A: For Development (Recommended)**
   - Copy `appsettings.Example.json` to `appsettings.Development.json`
   - Edit `appsettings.Development.json` with your database settings:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=YOUR_SERVER\\SQLEXPRESS;Integrated Security=SSPI;Initial Catalog=YOUR_DATABASE;Encrypt=false;app=SqlMcpServer"
     }
   }
   ```
   
   **Option B: Using Environment Variables**
   - Set the `ConnectionStrings__DefaultConnection` environment variable:
   ```bash
   # Windows Command Prompt
   set "ConnectionStrings__DefaultConnection=Data Source=YOUR_SERVER\SQLEXPRESS;Integrated Security=SSPI;Initial Catalog=YOUR_DATABASE;Encrypt=false;app=SqlMcpServer"
   
   # Windows PowerShell
   $env:ConnectionStrings__DefaultConnection="Data Source=YOUR_SERVER\SQLEXPRESS;Integrated Security=SSPI;Initial Catalog=YOUR_DATABASE;Encrypt=false;app=SqlMcpServer"
   
   # Linux/Mac
   export ConnectionStrings__DefaultConnection="Data Source=YOUR_SERVER;Integrated Security=SSPI;Initial Catalog=YOUR_DATABASE;Encrypt=false;app=SqlMcpServer"
   ```

   **Security Note**: The `appsettings.Development.json` file is excluded from git to keep your sensitive database credentials secure.

4. **Build the project**:
   ```bash
   dotnet build
   ```

## GitHub Repository Setup

This project is now ready for GitHub synchronization. To sync to GitHub:

### Option 1: Create New Repository on GitHub
1. **Go to GitHub** and create a new repository named `SqlMcpServer`
2. **Copy the repository URL** (e.g., `https://github.com/yourusername/SqlMcpServer.git`)
3. **Add remote origin and push**:
   ```bash
   git remote add origin https://github.com/yourusername/SqlMcpServer.git
   git branch -M main
   git push -u origin main
   ```

### Option 2: Using GitHub CLI (if installed)
```bash
gh repo create SqlMcpServer --public --source=. --remote=origin --push
```

### Option 3: Using VS Code
1. **Open VS Code** in this folder
2. **Press Ctrl+Shift+P** and type "Git: Publish to GitHub"
3. **Select repository type** (public/private)
4. **Name the repository** "SqlMcpServer"
5. **Click Publish**

The repository is already initialized with:
- ✅ Initial commit completed
- ✅ Proper .gitignore for .NET projects
- ✅ All source files staged and committed
- ✅ Solution renamed to match project name

## Running the Server

### Method 1: Direct Execution
```bash
dotnet run
```

### Method 2: Using VS Code
1. Open the project in VS Code
2. Press `F5` to run in debug mode
3. Or use `Ctrl+F5` to run without debugging

## MCP Client Configuration

To use this server with an MCP client (like GitHub Copilot), add the following configuration:

### For Claude Desktop or Other MCP Clients

Add to your MCP client configuration file (typically `claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "sql-mcp-server": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\path\\to\\your\\SqlMcpServer"],
      "env": {}
    }
  }
}
```

### For GitHub Copilot in VS Code

1. **Install the MCP extension** for VS Code (if available)
2. **Configure the server** in your VS Code settings or workspace configuration
3. **Use the provided mcp-config.json** as a reference

## Testing the Server

You can test the server using any MCP client or by building a simple test client. Here are some example queries you can try:

1. **Get first 10 actors**:
   - "Find the first 10 actors"
   - This will use the `GetActors` tool

2. **Search for specific actors**:
   - "Find actors with the name 'Smith'"
   - This will use the `SearchActors` tool

3. **Get a specific actor**:
   - "Get actor with ID 5"
   - This will use the `GetActorById` tool

4. **Custom SQL query**:
   - "Execute: SELECT TOP 5 * FROM actor WHERE first_name LIKE 'A%'"
   - This will use the `ExecuteCustomQuery` tool

## Example Questions You Can Ask

Once connected to an MCP client, you can ask natural language questions that will be automatically translated into the appropriate database queries. Here are practical examples organized by category:

### 🎭 Actor Queries (Customer/Supplier Management)
- "Who are the first 10 actors in the database?"
- "Find all actors with the last name 'Johnson'"
- "Show me actors whose first name starts with 'M'"
- "Get actor details for ID 42"
- "List actors with 'Anna' in their name"
- "How many actors are in the database?"
- "Search for actors named 'Smith'"
- "Show me actors 11-20 (pagination example)"
- "Find customers in Stockholm"
- "List all suppliers with phone numbers"
- "Show me inactive customer accounts"
- "Find actors created this year"

### 💰 Product and Pricing Queries
- "List all products in the database"
- "Show me the first 20 products with their descriptions"
- "Find products containing 'konsult' in the description"
- "Get pricing information for product number 1001"
- "Show all products with their current sale prices"
- "List products for customer price group 2"
- "What are the most expensive products available?"
- "Show product pricing matrix details"
- "Find products priced between 500 and 1000 SEK"
- "Which products have no pricing set?"
- "Show discontinued products"
- "List products by category or type"

### 📋 Orders and Sales Analysis
- "Show recent sales from the last 30 days"
- "What were the sales for last year?"
- "Get sales summary for current year"
- "Find orders for customer actor 12345"
- "Show orders from last year"
- "List order lines for order number 987"
- "Get detailed order information with actor details"
- "Show orders with delivery actor 5001"
- "Find orders by status code"
- "Which products sold best last year?"
- "Show orders above 10,000 SEK value"
- "Find incomplete or pending orders"
- "Analyze sales trends by quarter"

### 📄 Invoice and Document Management
- "Show invoice documents from last year"
- "Find invoices for liable actor 3456"
- "Get invoice details for document number 12345"
- "List invoices between specific dates"
- "Show delivery and billing information for invoices"
- "Find unpaid invoices older than 60 days"
- "Show invoices with payment terms"
- "List credit notes and adjustments"

### 🏢 Customer and Supplier Transactions
- "Show unpaid customer invoices"
- "List customer transactions for account 5001"
- "Find customer transactions from last year"
- "Show all payments from customer number 'CUST001'"
- "Get unpaid supplier transactions"
- "List supplier invoices from specific supplier"
- "Show supplier payments by transaction type"
- "Find overdue customer payments"
- "Show payment history for a customer"
- "Analyze cash flow from customers"
- "Find largest outstanding balances"

### 📊 Financial Dimensions Analysis
- "Analyze financial dimension R1 usage"
- "Show cost center breakdown (R2 dimension)"
- "Get project analysis from R3 dimension"
- "List all departments in R4 dimension"
- "Analyze dimension R7 with transaction totals"
- "Show comprehensive dimension analysis for R1"
- "Find which dimensions are actually being used"
- "Get financial dimension hierarchy for R5"
- "Compare costs across departments"
- "Show project profitability by R3"
- "Analyze spend by cost center"

### 🏦 Chart of Accounts and General Ledger
- "List all active accounts in the chart of accounts"
- "Show account transactions for last year"
- "Find accounts of type 'Revenue'"
- "Get general ledger entries for account 3000"
- "Show accounts containing 'sales' in the name"
- "List asset accounts only"
- "Get account transactions by transaction type"
- "Show account balances as of year-end"
- "Find accounts with high activity"
- "Analyze expense account trends"

### 💹 **Financial Reporting & Business Intelligence**
- "Generate a Profit & Loss statement for last year"
- "Show me the balance sheet as of year end"
- "Create a cash flow statement for last year"
- "Generate P&L with previous year comparison"
- "Show balance sheet including zero balance accounts"
- "Calculate our current financial ratios"
- "Show liquidity ratios with interpretations"
- "Analyze profitability ratios for the company"
- "Calculate debt-to-equity and leverage ratios"
- "Get comprehensive financial ratio analysis"

### ⏰ **Aging & Credit Management**
- "Show customer aging report for outstanding invoices"
- "Generate receivables aging analysis as of today"
- "Find customers with invoices over 90 days overdue"
- "Show aging breakdown by 30-day periods"
- "Analyze collection risk by customer"
- "Get detailed aging report with payment patterns"
- "Which customers are our biggest credit risks?"
- "Show payment behavior patterns"

### � **Profitability & Performance Analysis**
- "Analyze customer profitability for last year"
- "Show product profitability with margins"
- "Which customers are most profitable?"
- "Analyze gross margins by product category"
- "Compare customer profitability year-over-year"
- "Show top 20 most profitable products"
- "Find customers with declining margins"
- "Analyze sales performance by region"

### 📈 **Business Trends & Insights**
- "Show sales trends by month last year"
- "Analyze seasonal patterns in our business"
- "Compare revenue streams year-over-year"
- "Which products are growing vs declining?"
- "Show customer acquisition trends"
- "Analyze average order values over time"
- "Find our most loyal customers"
- "Show market share by product category"

### 🔍 **Custom SQL Queries**
- "Execute: SELECT TOP 10 * FROM actor WHERE last_name LIKE 'S%'"
- "Run custom query to find recent actors"
- "Execute custom analysis on actor table"
- "Show me raw SQL results for actor data"
- "Query specific tables directly"
- "Perform complex joins across multiple tables"

### 🎯 **Real Business Scenarios**
- "I need to prepare for our board meeting - show me key financial metrics"
- "Our CFO wants to know our working capital position"
- "Which customers should we focus our sales efforts on?"
- "Are there any red flags in our receivables?"
- "What's our cash conversion cycle?"
- "Show me our best performing products for procurement planning"
- "I need to analyze our cost structure for budgeting"
- "Which suppliers should we prioritize for payment?"
- "Help me understand our seasonal business patterns"
- "What's our customer concentration risk?"

### 📅 **Time-Based Analysis**
- "Compare Q1 this year vs Q1 last year performance"
- "Show monthly recurring revenue trends"
- "Analyze year-over-year growth rates"
- "Find seasonal peaks and valleys in sales"
- "Show quarterly financial summaries"
- "Track weekly sales performance"

### � **Advanced Analytics Questions**
- "What's our Days Sales Outstanding (DSO)?"
- "Calculate inventory turnover ratios"
- "Show me our cash conversion cycle"
- "Analyze customer lifetime value"
- "What's our gross margin by product line?"
- "Find our most efficient cost centers"
- "Show return on investment by project"
- "Analyze working capital efficiency"

### 🎯 **Decision Support Queries**
- "Should we extend credit to customer X?"
- "Which products should we discontinue?"
- "Where should we focus our collection efforts?"
- "What's our optimal inventory level?"
- "Which customers deserve volume discounts?"
- "How much credit exposure do we have?"
- "What's our break-even point by product?"

### 💼 **Management Reporting**
- "Prepare an executive dashboard summary"
- "Show key performance indicators"
- "Generate month-end financial summary"
- "Create a risk assessment report"
- "Show budget vs actual performance"
- "Prepare cash flow forecast data"
- "Generate audit trail reports"

### 💡 **Pro Tips for Better Results**
- **Include specific parameters**: "Show 25 products with offset 50"
- **Use date formats**: "From 20240101 to 20241231" (YYYYMMDD format)
- **Specify filter criteria**: "Only active accounts" or "Unpaid transactions only"
- **Request pagination**: "Show me the next 20 results"
- **Ask for totals**: "Include transaction counts and totals"
- **Request details**: "Show full details with related data"
- **Use natural language**: The MCP server understands conversational queries
- **Build on results**: "Now show me the details for order 12345"

## SQL Methodology and Transparency

When using this MCP server, all responses should include information about:

### 🔍 How Data Was Retrieved
- **MCP Tool Used**: Which specific tool was called (e.g., `get_products_with_pricing`, `get_orders`, etc.)
- **Parameters Applied**: What filters, limits, or search criteria were used
- **Underlying SQL Logic**: The equivalent SQL query concept behind the MCP tool call

### 📊 Example Response Format
When answering "What products cost between 500 and 1000?", a complete response should include:

1. **The Results**: Table or list of matching products
2. **How It Was Found**: 
   - Tool: `get_products_with_pricing` with `limit: 50`
   - SQL Equivalent: `SELECT * FROM Prod p LEFT JOIN PrDcMat pm ON p.ProdNo = pm.ProdNo WHERE pm.SalePrice BETWEEN 500 AND 1000`
   - Filtering Logic: Client-side filtering of results where `salePrice >= 500 AND salePrice <= 1000`

### 🛠️ Available MCP Tools and Their SQL Equivalents
- **get_products_with_pricing** ≈ `SELECT p.*, pm.SalePrice FROM Prod p LEFT JOIN PrDcMat pm ON p.ProdNo = pm.ProdNo`
- **get_orders** ≈ `SELECT * FROM Ord WHERE [filters]`
- **get_customer_transactions** ≈ `SELECT * FROM CustTr WHERE [filters]`
- **get_actors** ≈ `SELECT * FROM Actor WHERE [filters]`
- **execute_custom_query** ≈ Direct SQL execution with safety validations

**Technical Note**: All MCP tools execute direct SQL queries against the Visma Business database using Entity Framework Core, not through VBS/WCF API calls.

### 🎯 Best Practices for Responses
- Always explain which MCP tool was used
- Show the parameters that were applied
- Indicate any client-side filtering performed on the results
- Provide the SQL concept equivalent when possible
- Mention if multiple tool calls were needed for complex queries

## Visma Business Database Schema

This MCP server is designed to work with the standard **Visma Business SQL database schema**. Based on official Visma documentation, the database contains over 400 tables across multiple functional areas.

### 🏢 Core Business Entities
- **Actor (Table #152)** - **Unified actor management** for all business relationships: customers, suppliers, employees, contacts, prospects, and partners
- **Prod (Table #114)** - Product and service catalog with descriptions
- **PrDcMat (Table #116)** - Price and discount matrix for dynamic pricing

### 📋 Order Management System
- **Ord (Table #127)** - **All order types**: sales orders (SO), purchase orders (PO), quotes, service orders, returns, and internal transfers
- **OrdLn (Table #128)** - Order line items with detailed product information for all order types
- **OrdDoc (Table #335)** - Invoice documents and delivery notes across all order types
- **OrdDocLn (Table #336)** - Invoice line items with accounting details

### 💰 Financial Management Core
- **Ac (Table #65)** - General ledger chart of accounts
- **AcTr (Table #68)** - General ledger transactions
- **CustTr (Table #54)** - Customer-specific financial transactions
- **SupTr (Table #61)** - Supplier payment and invoice tracking
- **AcBal (Table #69)** - General ledger account balances

### 📊 Multi-Dimensional Financial Reporting (R1-R12 System)
**Organizational Units (Financial Dimensions):**
- **R1 (Table #36)** - Typically cost centers
- **R2 (Table #37)** - Typically departments  
- **R3 (Table #38)** - Typically projects
- **R4-R6 (Tables #39-41)** - Custom business dimensions
- **R7-R12 (Tables #257-262)** - Extended dimensions for complex organizations

**Associated Balance Tables:**
- **R1Bal-R12Bal (Tables #81-86, #251-256)** - Dimensional balance tracking
- Enables comprehensive cost center, project, and department reporting

### 🏦 Advanced Financial Features  
- **Currency management** - Multi-currency support with exchange rates
- **VAT/Tax handling** - Comprehensive tax code and VAT period management
- **Budget systems** - Budget planning and variance analysis
- **Audit trails** - Complete transaction history and change logging

### 🔍 Complete Table Reference (400+ Tables)
The Visma Business schema includes specialized tables for:
- **Workflow management** - Document approval and processing
- **Inventory control** - Stock balances, reservations, and movements
- **Banking integration** - Payment processing and reconciliation
- **Reporting framework** - Customizable business intelligence
- **User management** - Access control and permissions
- **Multi-company support** - Consolidated reporting across entities

### 📈 Business Intelligence Infrastructure
**Pre-built Analytics Support:**
- Sales analytics and customer insights
- Financial consolidation and reporting  
- Project profitability analysis
- Multi-dimensional cost analysis
- Audit trail and compliance reporting

### 🔧 Technical Implementation Notes
```sql
-- Example key table relationships
Actor: ActorId (Primary), ActorNo, Name, Address, etc.
Ord: OrderNo (Primary), ActorId (FK to Actor), OrderDate, Status
OrdLn: OrderNo (FK to Ord), LineNo, ProdNo (FK to Prod), Quantity, Price
AcTr: VoucherNo, AccountNo (FK to Ac), Amount, R1-R12 dimensions

-- Financial dimension example
R1: R1Id (Primary), R1No, Description, ParentR1Id (hierarchical)
R1Bal: R1Id (FK), AccountNo (FK), Period, DebitAmount, CreditAmount
```

This comprehensive schema enables sophisticated business intelligence queries through the MCP server's natural language interface.

## Security Considerations

- **Read-Only Access**: The server only allows SELECT queries for security
- **Parameterized Queries**: All user inputs are properly parameterized to prevent SQL injection
- **Input Validation**: All inputs are validated before database execution
- **Comprehensive Business Data Access**: Supports all major Visma Business entities while maintaining security
- **Configuration Security**: Sensitive database credentials are kept in local configuration files that are excluded from version control
- **Visma Business Compliance**: Respects Visma Business security models and user permissions
- **Audit Trail Preservation**: Read-only operations ensure no modification of business data or audit logs

### Configuration File Security
- `appsettings.json` - Safe for repository (contains example/default values)
- `appsettings.Development.json` - **EXCLUDED from git** (contains real credentials)
- `appsettings.Example.json` - Template file for easy setup
- Environment variables - Secure alternative for production deployments

## Error Handling

The server includes comprehensive error handling:
- Database connection errors
- Invalid query parameters
- SQL execution errors
- Malformed requests

All errors are returned as JSON responses with descriptive error messages.

## Extending the Server

To add support for additional Visma Business database tables or custom business logic:

1. **Create new model classes** in the `Models` folder that match your Visma Business schema
2. **Add DbSet properties** to `SqlMcpDbContext` for the new Visma Business tables
3. **Create new tool methods** in `DatabaseTools` class following Visma Business naming conventions
4. **Mark methods** with `[McpServerTool]` and `[Description]` attributes
5. **Consider Visma Business relationships** when designing queries (Actor relationships, dimension hierarchies, etc.)

### Common Visma Business Extensions
- Custom dimension tables beyond R1-R12
- Company-specific product categorizations  
- Custom financial reporting structures
- Integration with Visma Business workflows

## Troubleshooting

### Database Connection Issues
- Verify SQL Server is running and accessible
- Check the connection string format matches your Visma Business installation
- Ensure the Visma Business database exists and is accessible
- Verify Windows Authentication or SQL Authentication permissions
- Confirm network connectivity to the Visma Business database server
- Check if Visma Business services are running

### MCP Client Connection Issues
- Ensure the server is running before connecting the client
- Check that the command path in the MCP configuration is correct
- Verify that .NET 8.0 is installed and accessible

### Performance Issues
- Consider adding database indexes for frequently queried columns
- Adjust the default and maximum limits for queries
- Monitor memory usage for large result sets

## License

This project is provided as-is for educational and development purposes.

## Contributing

To contribute to this project:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Support

For issues and questions:
1. Check the troubleshooting section above
2. Review the error messages in the console output
3. Verify your database connection and schema
4. Ensure all prerequisites are met
