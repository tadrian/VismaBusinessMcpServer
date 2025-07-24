using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Server;
using SqlMcpServer.Data;
using SqlMcpServer.Models;

namespace SqlMcpServer.Tools;

/// <summary>
/// MCP tools for database operations
/// </summary>
[McpServerToolType]
public class DatabaseTools
{
    private readonly SqlMcpDbContext _context;

    public DatabaseTools(SqlMcpDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets JSON serialization options that handle circular references
    /// </summary>
    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Retrieves actors from the database with optional limit and offset
    /// </summary>
    /// <param name="limit">Maximum number of actors to return (default: 10)</param>
    /// <param name="offset">Number of actors to skip (default: 0)</param>
    /// <returns>JSON array of actor objects</returns>
    [McpServerTool]
    [Description("Retrieves actors from the database. Use this to find actors with optional pagination.")]
    public async Task<string> GetActors(
        [Description("Maximum number of actors to return (default: 10, max: 100)")]
        int limit = 10,
        [Description("Number of actors to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Validate input parameters
            if (limit <= 0 || limit > 100)
            {
                limit = 10;
            }

            if (offset < 0)
            {
                offset = 0;
            }

            var actors = await _context.Actors
                .OrderBy(a => a.ActorId)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var result = new
            {
                actors = actors,
                count = actors.Count,
                offset = offset,
                limit = limit,
                total = await _context.Actors.CountAsync()
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Searches for actors by name (first name or last name)
    /// </summary>
    /// <param name="searchTerm">Search term to match against first name or last name</param>
    /// <param name="limit">Maximum number of results to return (default: 10)</param>
    /// <returns>JSON array of matching actor objects</returns>
    [McpServerTool]
    [Description("Searches for actors by name. Searches both first and last names for the given term.")]
    public async Task<string> SearchActors(
        [Description("Search term to find actors by name")]
        string searchTerm,
        [Description("Maximum number of results to return (default: 10, max: 100)")]
        int limit = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Search term is required" 
                });
            }

            // Validate limit
            if (limit <= 0 || limit > 100)
            {
                limit = 10;
            }

            var actors = await _context.Actors
                .Where(a => a.Name.Contains(searchTerm) || 
                           a.Address1.Contains(searchTerm))
                .OrderBy(a => a.Name)
                .ThenBy(a => a.Address1)
                .Take(limit)
                .ToListAsync();

            var result = new
            {
                actors = actors,
                searchTerm = searchTerm,
                count = actors.Count,
                limit = limit
            };

            return JsonSerializer.Serialize(result, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets a specific actor by ID
    /// </summary>
    /// <param name="actorId">The ID of the actor to retrieve</param>
    /// <returns>JSON object with actor details</returns>
    [McpServerTool]
    [Description("Retrieves a specific actor by their ID.")]
    public async Task<string> GetActorById(
        [Description("The unique ID of the actor to retrieve")]
        int actorId)
    {
        try
        {
            var actor = await _context.Actors
                .FirstOrDefaultAsync(a => a.ActorId == actorId);

            if (actor == null)
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Actor not found", 
                    actorId = actorId 
                });
            }

            return JsonSerializer.Serialize(actor, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Executes a custom SQL query on the actors table
    /// </summary>
    /// <param name="sqlQuery">The SQL query to execute (SELECT statements only)</param>
    /// <returns>JSON result of the query</returns>
    [McpServerTool]
    [Description("Executes a custom SQL query on the actors table. Only SELECT statements are allowed for security.")]
    public async Task<string> ExecuteCustomQuery(
        [Description("SQL SELECT query to execute on the actors table")]
        string sqlQuery)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(sqlQuery))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "SQL query is required" 
                });
            }

            // Security check: only allow SELECT statements
            var trimmedQuery = sqlQuery.Trim().ToUpperInvariant();
            if (!trimmedQuery.StartsWith("SELECT"))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Only SELECT statements are allowed" 
                });
            }

            // Use a more flexible approach - execute raw SQL and return dynamic results
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sqlQuery;
            
            await _context.Database.OpenConnectionAsync();
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object?>>();
            
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[fieldName] = value;
                }
                results.Add(row);
            }

            return JsonSerializer.Serialize(new
            {
                query = sqlQuery,
                results = results,
                count = results.Count
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Query execution error", 
                message = ex.Message,
                query = sqlQuery
            });
        }
    }

    /// <summary>
    /// Gets sales data for a specific year
    /// </summary>
    /// <param name="year">Year to get sales for (e.g., 2023)</param>
    /// <param name="limit">Maximum number of results to return (default: 100)</param>
    /// <returns>JSON array of sales data</returns>
    [McpServerTool]
    [Description("Gets sales data for a specific year. Use this to analyze sales performance by year.")]
    public async Task<string> GetSalesByYear(
        [Description("Year to get sales for (e.g., 2023)")]
        int year,
        [Description("Maximum number of results to return (default: 100, max: 1000)")]
        int limit = 100)
    {
        try
        {
            // Validate input parameters
            if (limit <= 0 || limit > 1000)
            {
                limit = 100;
            }

            // Convert year to the date format used in the database (YYYYMMDD)
            var yearStart = year * 10000 + 101; // e.g., 20230101
            var yearEnd = year * 10000 + 1231;  // e.g., 20231231

            var salesData = await _context.OrderDocumentLines
                .Where(odl => odl.TransactionDate >= yearStart && odl.TransactionDate <= yearEnd)
                .Include(odl => odl.OrderDocument)
                .Include(odl => odl.Product)
                .Select(odl => new
                {
                    OrderDocumentNumber = odl.OrderDocumentNumber,
                    LineNumber = odl.DocumentLineNumber,
                    TransactionDate = odl.TransactionDate,
                    ProductNumber = odl.ProductNumber,
                    ProductDescription = odl.Product != null ? odl.Product.Description : null,
                    Quantity = odl.ExecutedQuantity,
                    Price = odl.Price,
                    Amount = odl.Amount,
                    VatAmount = odl.VatAmount
                })
                .OrderByDescending(x => x.TransactionDate)
                .Take(limit)
                .ToListAsync();

            var totalSales = await _context.OrderDocumentLines
                .Where(odl => odl.TransactionDate >= yearStart && odl.TransactionDate <= yearEnd && odl.Amount.HasValue)
                .SumAsync(odl => odl.Amount!.Value);

            return JsonSerializer.Serialize(new
            {
                year = year,
                salesData = salesData,
                totalSales = totalSales,
                count = salesData.Count,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets sales summary by year with totals
    /// </summary>
    /// <param name="year">Year to get summary for (e.g., 2023)</param>
    /// <returns>JSON object with sales summary</returns>
    [McpServerTool]
    [Description("Gets sales summary for a specific year with totals and statistics.")]
    public async Task<string> GetSalesSummaryByYear(
        [Description("Year to get summary for (e.g., 2023)")]
        int year)
    {
        try
        {
            // Convert year to the date format used in the database (YYYYMMDD)
            var yearStart = year * 10000 + 101; // e.g., 20230101
            var yearEnd = year * 10000 + 1231;  // e.g., 20231231

            var summary = await _context.OrderDocumentLines
                .Where(odl => odl.TransactionDate >= yearStart && odl.TransactionDate <= yearEnd)
                .GroupBy(odl => 1) // Group all into one result
                .Select(g => new
                {
                    TotalSales = g.Sum(odl => odl.Amount ?? 0),
                    TotalVat = g.Sum(odl => odl.VatAmount ?? 0),
                    TotalQuantity = g.Sum(odl => odl.ExecutedQuantity ?? 0),
                    OrderCount = g.Count(),
                    UniqueProducts = g.Select(odl => odl.ProductNumber).Distinct().Count()
                })
                .FirstOrDefaultAsync();

            // Get top products by sales amount
            var topProducts = await _context.OrderDocumentLines
                .Where(odl => odl.TransactionDate >= yearStart && odl.TransactionDate <= yearEnd && odl.Amount.HasValue)
                .Include(odl => odl.Product)
                .GroupBy(odl => new { odl.ProductNumber, ProductDescription = odl.Product!.Description })
                .Select(g => new
                {
                    ProductNumber = g.Key.ProductNumber,
                    ProductDescription = g.Key.ProductDescription,
                    TotalSales = g.Sum(odl => odl.Amount!.Value),
                    TotalQuantity = g.Sum(odl => odl.ExecutedQuantity ?? 0),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSales)
                .Take(10)
                .ToListAsync();

            return JsonSerializer.Serialize(new
            {
                year = year,
                summary = summary,
                topProducts = topProducts
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets products with optional search and pagination
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter products by number or description</param>
    /// <param name="limit">Maximum number of products to return (default: 20)</param>
    /// <param name="offset">Number of products to skip (default: 0)</param>
    /// <returns>JSON array of product objects</returns>
    [McpServerTool]
    [Description("Gets products from the database with optional search and pagination.")]
    public async Task<string> GetProducts(
        [Description("Optional search term to filter products by number or description")]
        string? searchTerm = null,
        [Description("Maximum number of products to return (default: 20, max: 100)")]
        int limit = 20,
        [Description("Number of products to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Validate input parameters
            if (limit <= 0 || limit > 100)
            {
                limit = 20;
            }

            if (offset < 0)
            {
                offset = 0;
            }

            var query = _context.Products.AsQueryable();

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(p => p.ProductNumber.Contains(searchTerm) || 
                                        p.Description.Contains(searchTerm));
            }

            var products = await query
                .OrderBy(p => p.ProductNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            return JsonSerializer.Serialize(new
            {
                products = products,
                count = products.Count,
                total = totalCount,
                offset = offset,
                limit = limit,
                searchTerm = searchTerm
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets recent sales transactions
    /// </summary>
    /// <param name="days">Number of days back to look for transactions (default: 30)</param>
    /// <param name="limit">Maximum number of transactions to return (default: 50)</param>
    /// <returns>JSON array of recent sales transactions</returns>
    [McpServerTool]
    [Description("Gets recent sales transactions from the last N days.")]
    public async Task<string> GetRecentSales(
        [Description("Number of days back to look for transactions (default: 30)")]
        int days = 30,
        [Description("Maximum number of transactions to return (default: 50, max: 200)")]
        int limit = 50)
    {
        try
        {
            // Validate input parameters
            if (limit <= 0 || limit > 200)
            {
                limit = 50;
            }

            if (days <= 0)
            {
                days = 30;
            }

            // Calculate the date threshold in YYYYMMDD format
            var thresholdDate = DateTime.Now.AddDays(-days);
            var dateThreshold = thresholdDate.Year * 10000 + thresholdDate.Month * 100 + thresholdDate.Day;

            var recentSales = await _context.OrderDocumentLines
                .Where(odl => odl.TransactionDate >= dateThreshold)
                .Include(odl => odl.OrderDocument)
                .Include(odl => odl.Product)
                .Select(odl => new
                {
                    OrderDocumentNumber = odl.OrderDocumentNumber,
                    LineNumber = odl.DocumentLineNumber,
                    TransactionDate = odl.TransactionDate,
                    ProductNumber = odl.ProductNumber,
                    ProductDescription = odl.Product != null ? odl.Product.Description : null,
                    Quantity = odl.ExecutedQuantity,
                    Price = odl.Price,
                    Amount = odl.Amount,
                    VatAmount = odl.VatAmount
                })
                .OrderByDescending(x => x.TransactionDate)
                .Take(limit)
                .ToListAsync();

            return JsonSerializer.Serialize(new
            {
                days = days,
                recentSales = recentSales,
                count = recentSales.Count,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets pricing information for a specific product
    /// </summary>
    /// <param name="productNumber">Product number to get pricing for</param>
    /// <param name="customerPriceGroup">Optional customer price group filter</param>
    /// <returns>JSON object with pricing information</returns>
    [McpServerTool]
    [Description("Gets pricing information for a specific product from the price matrix.")]
    public async Task<string> GetProductPricing(
        [Description("Product number to get pricing for")]
        string productNumber,
        [Description("Optional customer price group filter (default: all groups)")]
        int? customerPriceGroup = null)
    {
        try
        {
            var query = _context.PriceMatrix
                .Where(pm => pm.ProductNumber == productNumber);

            // Apply customer price group filter if specified
            if (customerPriceGroup.HasValue)
            {
                query = query.Where(pm => pm.CustomerPriceGroup == customerPriceGroup.Value || pm.CustomerPriceGroup == null);
            }

            var pricing = await query
                .Include(pm => pm.Product)
                .Select(pm => new
                {
                    ProductNumber = pm.ProductNumber,
                    ProductDescription = pm.Product != null ? pm.Product.Description : null,
                    CustomerPriceGroup = pm.CustomerPriceGroup,
                    ProductPriceGroup = pm.ProductPriceGroup,
                    SalePrice = pm.SalePrice,
                    SaleDiscountPercent = pm.SaleDiscountPercent,
                    SaleDiscountAmount = pm.SaleDiscountAmount,
                    CompanyPrice = pm.CompanyPrice,
                    SuggestedPrice = pm.SuggestedPrice,
                    CostPrice = pm.CostPrice,
                    PurchasePrice = pm.PurchasePrice,
                    LineNumber = pm.LineNumber
                })
                .OrderBy(p => p.CustomerPriceGroup)
                .ToListAsync();

            return JsonSerializer.Serialize(new
            {
                productNumber = productNumber,
                customerPriceGroup = customerPriceGroup,
                pricing = pricing,
                count = pricing.Count
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets all products with their current pricing
    /// </summary>
    /// <param name="limit">Maximum number of products to return (default: 20)</param>
    /// <param name="offset">Number of products to skip (default: 0)</param>
    /// <param name="customerPriceGroup">Optional customer price group filter</param>
    /// <returns>JSON array of products with pricing</returns>
    [McpServerTool]
    [Description("Gets all products with their current pricing from the price matrix.")]
    public async Task<string> GetProductsWithPricing(
        [Description("Maximum number of products to return (default: 20, max: 100)")]
        int limit = 20,
        [Description("Number of products to skip for pagination (default: 0)")]
        int offset = 0,
        [Description("Optional customer price group filter")]
        int? customerPriceGroup = null)
    {
        try
        {
            // Validate input parameters
            if (limit <= 0 || limit > 100)
            {
                limit = 20;
            }

            if (offset < 0)
            {
                offset = 0;
            }

            var query = from p in _context.Products
                        join pm in _context.PriceMatrix on p.ProductNumber equals pm.ProductNumber into priceJoin
                        from pricing in priceJoin.DefaultIfEmpty()
                        where pricing == null || pricing.ProductNumber != null
                        select new
                        {
                            ProductNumber = p.ProductNumber,
                            ProductDescription = p.Description,
                            ProductGroup = p.ProductGroup,
                            ProductType = p.ProductType,
                            CustomerPriceGroup = pricing != null ? pricing.CustomerPriceGroup : (int?)null,
                            SalePrice = pricing != null ? pricing.SalePrice : (decimal?)null,
                            SaleDiscountPercent = pricing != null ? pricing.SaleDiscountPercent : (decimal?)null,
                            CompanyPrice = pricing != null ? pricing.CompanyPrice : (decimal?)null,
                            SuggestedPrice = pricing != null ? pricing.SuggestedPrice : (decimal?)null,
                            CostPrice = pricing != null ? pricing.CostPrice : (decimal?)null
                        };

            // Apply customer price group filter if specified
            if (customerPriceGroup.HasValue)
            {
                query = query.Where(p => p.CustomerPriceGroup == customerPriceGroup.Value || p.CustomerPriceGroup == null);
            }

            var productsWithPricing = await query
                .OrderBy(p => p.ProductNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var totalCount = await _context.Products.CountAsync();

            return JsonSerializer.Serialize(new
            {
                products = productsWithPricing,
                count = productsWithPricing.Count,
                total = totalCount,
                offset = offset,
                limit = limit,
                customerPriceGroup = customerPriceGroup
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // FINANCIAL DIMENSION TOOLS (R1-R12)

    /// <summary>
    /// Gets financial dimension data from any R table (R1-R12)
    /// </summary>
    /// <param name="dimensionTable">Dimension table name (R1, R2, ..., R12)</param>
    /// <param name="searchTerm">Optional search term to filter by name or description</param>
    /// <param name="limit">Maximum number of records to return (default: 50)</param>
    /// <param name="offset">Number of records to skip (default: 0)</param>
    /// <returns>JSON array of dimension objects</returns>
    [McpServerTool]
    [Description("Gets financial dimension data from dimension tables R1-R12. Use this to explore financial dimensions like departments, cost centers, projects, etc.")]
    public async Task<string> GetFinancialDimensions(
        [Description("Dimension table name (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)")]
        string dimensionTable,
        [Description("Optional search term to filter by name or description")]
        string? searchTerm = null,
        [Description("Maximum number of records to return (default: 50, max: 100)")]
        int limit = 50,
        [Description("Number of records to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Validate dimension table name
            var validTables = new[] { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12" };
            if (!validTables.Contains(dimensionTable.ToUpper()))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Invalid dimension table", 
                    message = $"Dimension table must be one of: {string.Join(", ", validTables)}" 
                });
            }

            // Limit validation
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            object dimensions;
            
            switch (dimensionTable.ToUpper())
            {
                case "R1":
                    var r1Query = _context.DimensionsR1.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r1Query = r1Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r1Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R2":
                    var r2Query = _context.DimensionsR2.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r2Query = r2Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r2Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R3":
                    var r3Query = _context.DimensionsR3.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r3Query = r3Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r3Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R4":
                    var r4Query = _context.DimensionsR4.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r4Query = r4Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r4Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R5":
                    var r5Query = _context.DimensionsR5.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r5Query = r5Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r5Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R6":
                    var r6Query = _context.DimensionsR6.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r6Query = r6Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r6Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R7":
                    var r7Query = _context.DimensionsR7.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r7Query = r7Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r7Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R8":
                    var r8Query = _context.DimensionsR8.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r8Query = r8Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r8Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R9":
                    var r9Query = _context.DimensionsR9.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r9Query = r9Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r9Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R10":
                    var r10Query = _context.DimensionsR10.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r10Query = r10Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r10Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R11":
                    var r11Query = _context.DimensionsR11.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r11Query = r11Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r11Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                case "R12":
                    var r12Query = _context.DimensionsR12.AsQueryable();
                    if (!string.IsNullOrEmpty(searchTerm))
                        r12Query = r12Query.Where(d => d.Name!.Contains(searchTerm));
                    dimensions = await r12Query.Skip(offset).Take(limit).ToListAsync();
                    break;
                default:
                    return JsonSerializer.Serialize(new 
                    { 
                        error = "Invalid dimension table", 
                        message = $"Dimension table {dimensionTable} is not supported" 
                    });
            }

            return JsonSerializer.Serialize(new
            {
                dimensionTable = dimensionTable.ToUpper(),
                dimensions = dimensions,
                searchTerm = searchTerm,
                offset = offset,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ACCOUNTS AND GENERAL LEDGER TOOLS

    /// <summary>
    /// Gets chart of accounts with optional search and pagination
    /// </summary>
    /// <param name="searchTerm">Optional search term to filter accounts by number, name, or type</param>
    /// <param name="accountType">Optional account type filter</param>
    /// <param name="activeOnly">Only return active accounts (default: true)</param>
    /// <param name="limit">Maximum number of accounts to return (default: 50)</param>
    /// <param name="offset">Number of accounts to skip (default: 0)</param>
    /// <returns>JSON array of account objects</returns>
    [McpServerTool]
    [Description("Gets chart of accounts from the database with optional filtering and pagination.")]
    public async Task<string> GetAccounts(
        [Description("Optional search term to filter accounts by number, name, or type")]
        string? searchTerm = null,
        [Description("Optional account type filter")]
        string? accountType = null,
        [Description("Only return active accounts (default: true)")]
        bool activeOnly = true,
        [Description("Maximum number of accounts to return (default: 50, max: 100)")]
        int limit = 50,
        [Description("Number of accounts to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            var query = _context.Accounts.AsQueryable();

            if (activeOnly)
                query = query.Where(a => a.Suspended != 1);  // Use Suspended instead of Active

            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(a => a.AccountNumber.ToString().Contains(searchTerm) || 
                                        (a.Name != null && a.Name.Contains(searchTerm)) ||
                                        (a.AccountGroup != null && a.AccountGroup.Contains(searchTerm)));

            if (!string.IsNullOrEmpty(accountType))
                query = query.Where(a => a.AccountGroup == accountType);  // Use AccountGroup instead of AccountType

            var accounts = await query
                .OrderBy(a => a.AccountNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            return JsonSerializer.Serialize(new
            {
                accounts = accounts,
                count = accounts.Count,
                total = totalCount,
                offset = offset,
                limit = limit,
                searchTerm = searchTerm,
                accountType = accountType,
                activeOnly = activeOnly
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets account transactions (general ledger entries) with filtering
    /// </summary>
    /// <param name="accountNumber">Optional account number to filter by</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="transactionType">Optional transaction type filter</param>
    /// <param name="limit">Maximum number of transactions to return (default: 100)</param>
    /// <param name="offset">Number of transactions to skip (default: 0)</param>
    /// <returns>JSON array of account transaction objects</returns>
    [McpServerTool]
    [Description("Gets account transactions (general ledger entries) with optional filtering by account, date range, and transaction type.")]
    public async Task<string> GetAccountTransactions(
        [Description("Optional account number to filter by")]
        string? accountNumber = null,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Optional transaction type filter")]
        string? transactionType = null,
        [Description("Maximum number of transactions to return (default: 100, max: 200)")]
        int limit = 100,
        [Description("Number of transactions to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 200) limit = 200;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            var query = _context.AccountTransactions.AsQueryable();

            if (!string.IsNullOrEmpty(accountNumber) && int.TryParse(accountNumber, out int accountNum))
                query = query.Where(t => t.AccountNumber == accountNum);

            if (fromDate.HasValue)
                query = query.Where(t => t.VoucherDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.VoucherDate <= toDate.Value);

            if (!string.IsNullOrEmpty(transactionType))
                query = query.Where(t => t.VoucherType.ToString() == transactionType);

            var transactions = await query
                .Include(t => t.Account)
                .OrderByDescending(t => t.VoucherDate)
                .ThenByDescending(t => t.JournalNumber)
                .ThenByDescending(t => t.EntryNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            // Calculate totals
            var totalDebits = await query.Where(t => t.Credit == 0).SumAsync(t => t.AccountAmount);
            var totalCredits = await query.Where(t => t.Credit == 1).SumAsync(t => t.AccountAmount);

            return JsonSerializer.Serialize(new
            {
                transactions = transactions,
                count = transactions.Count,
                total = totalCount,
                totals = new
                {
                    debits = totalDebits,
                    credits = totalCredits,
                    net = totalDebits - totalCredits
                },
                filters = new
                {
                    accountNumber = accountNumber,
                    fromDate = fromDate,
                    toDate = toDate,
                    transactionType = transactionType
                },
                offset = offset,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ORDER MANAGEMENT TOOLS (Ord and OrdLn tables)

    /// <summary>
    /// Gets orders from the Ord table with filtering and pagination
    /// </summary>
    /// <param name="orderNumber">Optional order number to search for</param>
    /// <param name="deliveryActor">Optional delivery actor number filter</param>
    /// <param name="liableActor">Optional liable actor number filter</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="status">Optional order status filter</param>
    /// <param name="limit">Maximum number of orders to return (default: 50)</param>
    /// <param name="offset">Number of orders to skip (default: 0)</param>
    /// <returns>JSON array of order objects</returns>
    [McpServerTool]
    [Description("Gets orders from the Ord table with optional filtering by order number, actors, date range, and status.")]
    public async Task<string> GetOrders(
        [Description("Optional order number to search for")]
        int? orderNumber = null,
        [Description("Optional delivery actor number filter")]
        int? deliveryActor = null,
        [Description("Optional liable actor number filter")]
        int? liableActor = null,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Optional order status filter")]
        int? status = null,
        [Description("Maximum number of orders to return (default: 50, max: 100)")]
        int limit = 50,
        [Description("Number of orders to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            var query = _context.Orders.AsQueryable();

            if (orderNumber.HasValue)
                query = query.Where(o => o.OrderNumber == orderNumber.Value);

            if (deliveryActor.HasValue)
                query = query.Where(o => o.DeliveryActorNumber == deliveryActor.Value);

            if (liableActor.HasValue)
                query = query.Where(o => o.LiableActorNumber == liableActor.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            // Create a projection to avoid circular references
            var ordersWithSimpleLines = orders.Select(o => new
            {
                o.OrderNumber,
                o.OrderDate,
                o.DeliveryActorNumber,
                o.LiableActorNumber,
                o.ShipActorNumber,
                o.Status,
                o.OrderType,
                o.Currency,
                o.CustomerNumber,
                o.CustomerName,
                o.Reference,
                o.Amount,
                o.VatAmount,
                o.DeliveryDate,
                o.FactoryNumber,
                OrderLineCount = _context.OrderLines.Count(ol => ol.OrderNumber == o.OrderNumber)
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                orders = ordersWithSimpleLines,
                count = ordersWithSimpleLines.Count,
                total = totalCount,
                filters = new
                {
                    orderNumber = orderNumber,
                    deliveryActor = deliveryActor,
                    liableActor = liableActor,
                    fromDate = fromDate,
                    toDate = toDate,
                    status = status
                },
                offset = offset,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets order lines from the OrdLn table with filtering and financial dimension analysis
    /// </summary>
    /// <param name="orderNumber">Optional order number to filter by</param>
    /// <param name="productNumber">Optional product number filter</param>
    /// <param name="r1">Optional R1 dimension filter</param>
    /// <param name="r2">Optional R2 dimension filter</param>
    /// <param name="r3">Optional R3 dimension filter</param>
    /// <param name="r7">Optional R7 dimension filter</param>
    /// <param name="limit">Maximum number of order lines to return (default: 100)</param>
    /// <param name="offset">Number of order lines to skip (default: 0)</param>
    /// <returns>JSON array of order line objects with financial dimensions</returns>
    [McpServerTool]
    [Description("Gets order lines from the OrdLn table with optional filtering by order, product, and financial dimensions (R1-R12).")]
    public async Task<string> GetOrderLines(
        [Description("Optional order number to filter by")]
        int? orderNumber = null,
        [Description("Optional product number filter")]
        string? productNumber = null,
        [Description("Optional R1 dimension filter")]
        int? r1 = null,
        [Description("Optional R2 dimension filter")]
        int? r2 = null,
        [Description("Optional R3 dimension filter")]
        int? r3 = null,
        [Description("Optional R7 dimension filter")]
        string? r7 = null,
        [Description("Maximum number of order lines to return (default: 100, max: 200)")]
        int limit = 100,
        [Description("Number of order lines to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 200) limit = 200;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            var query = _context.OrderLines.AsQueryable();

            if (orderNumber.HasValue)
                query = query.Where(ol => ol.OrderNumber == orderNumber.Value);

            if (!string.IsNullOrEmpty(productNumber))
                query = query.Where(ol => ol.ProductNumber == productNumber);

            if (r1.HasValue)
                query = query.Where(ol => ol.R1 == r1.Value);

            if (r2.HasValue)
                query = query.Where(ol => ol.R2 == r2.Value);

            if (r3.HasValue)
                query = query.Where(ol => ol.R3 == r3.Value);

            if (!string.IsNullOrEmpty(r7))
                query = query.Where(ol => ol.R7 == r7);

            var orderLines = await query
                .Include(ol => ol.Product)
                .OrderByDescending(ol => ol.OrderNumber)
                .ThenBy(ol => ol.LineNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var totalCount = await query.CountAsync();

            // Calculate totals
            var totalAmount = await query.SumAsync(ol => ol.Amount);
            var totalQuantity = await query.SumAsync(ol => ol.NoRegistered);
            var totalDeliveredQuantity = await query.SumAsync(ol => ol.NoInvoiced);

            // Create projection to avoid circular reference
            var orderLinesProjection = orderLines.Select(ol => new
            {
                ol.OrderNumber,
                ol.LineNumber,
                ol.ProductNumber,
                Product = ol.Product != null ? new { ol.Product.ProductNumber, ol.Product.Description } : null,
                ol.NoRegistered,
                ol.NoInvoiced,
                ol.Price,
                ol.Amount,
                ol.VatAmount,
                ol.Description,
                ol.DeliveryDate,
                FinancialDimensions = new
                {
                    r1 = ol.R1,
                    r2 = ol.R2,
                    r3 = ol.R3,
                    r4 = ol.R4,
                    r5 = ol.R5,
                    r6 = ol.R6,
                    r7 = ol.R7,
                    r8 = ol.R8,
                    r9 = ol.R9,
                    r10 = ol.R10,
                    r11 = ol.R11,
                    r12 = ol.R12
                }
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                orderLines = orderLinesProjection,
                count = orderLinesProjection.Count,
                total = totalCount,
                totals = new
                {
                    amount = totalAmount,
                    quantity = totalQuantity,
                    deliveredQuantity = totalDeliveredQuantity
                },
                filters = new
                {
                    orderNumber = orderNumber,
                    productNumber = productNumber,
                    r1 = r1,
                    r2 = r2,
                    r3 = r3,
                    r7 = r7
                },
                offset = offset,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // INVOICE MANAGEMENT TOOLS (Enhanced OrdDoc tools)

    /// <summary>
    /// Gets order documents with filtering and pagination
    /// </summary>
    /// <param name="orderNumber">Optional order number to search for</param>
    /// <param name="deliveryActor">Optional delivery actor number filter</param>
    /// <param name="liableActor">Optional liable actor number filter</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="limit">Maximum number of orders to return (default: 50)</param>
    /// <param name="offset">Number of orders to skip (default: 0)</param>
    /// <returns>JSON array of order document objects</returns>
    [McpServerTool]
    [Description("Gets invoice documents (OrdDoc table) with optional filtering by document number, actors, and date range.")]
    public async Task<string> GetInvoiceDocuments(
        [Description("Optional invoice document number to search for")]
        int? orderNumber = null,
        [Description("Optional delivery actor number filter")]
        int? deliveryActor = null,
        [Description("Optional liable actor number filter")]
        int? liableActor = null,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Maximum number of invoices to return (default: 50, max: 100)")]
        int limit = 50,
        [Description("Number of invoices to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            var query = _context.OrderDocuments.AsQueryable();

            if (orderNumber.HasValue)
                query = query.Where(o => o.OrderDocumentNumber == orderNumber.Value);

            if (deliveryActor.HasValue)
                query = query.Where(o => o.DeliveryActorNumber == deliveryActor.Value);

            if (liableActor.HasValue)
                query = query.Where(o => o.LiableActorNumber == liableActor.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.DocumentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.DocumentDate <= toDate.Value);

            var invoices = await query
                .OrderByDescending(o => o.DocumentDate)
                .ThenByDescending(o => o.OrderDocumentNumber)
                .Skip(offset)
                .Take(limit)
                .Select(o => new
                {
                    o.OrderDocumentNumber,
                    o.DocumentType,
                    o.DocumentDate,
                    o.DeliveryActorNumber,
                    o.LiableActorNumber,
                    o.ShipActorNumber,
                    OrderLinesCount = o.OrderLines.Count()
                })
                .ToListAsync();

            var totalCount = await query.CountAsync();

            return JsonSerializer.Serialize(new
            {
                invoices = invoices,
                count = invoices.Count,
                total = totalCount,
                filters = new
                {
                    orderNumber = orderNumber,
                    deliveryActor = deliveryActor,
                    liableActor = liableActor,
                    fromDate = fromDate,
                    toDate = toDate
                },
                offset = offset,
                limit = limit
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets financial dimension analysis across all order lines (both Ord and OrdDoc)
    /// </summary>
    /// <param name="dimensionLevel">Dimension level to analyze (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="limit">Maximum number of dimension values to return (default: 50)</param>
    /// <returns>JSON array with dimension analysis</returns>
    [McpServerTool]
    [Description("Analyzes financial dimensions across order lines, showing totals by dimension value. Useful for cost center, project, or department analysis.")]
    public async Task<string> GetFinancialDimensionAnalysis(
        [Description("Dimension level to analyze (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)")]
        string dimensionLevel,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Maximum number of dimension values to return (default: 50, max: 100)")]
        int limit = 50)
    {
        try
        {
            // Validate dimension level
            var validDimensions = new[] { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12" };
            if (!validDimensions.Contains(dimensionLevel.ToUpper()))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Invalid dimension level", 
                    message = $"Dimension level must be one of: {string.Join(", ", validDimensions)}" 
                });
            }

            // Limit validation
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;

            var query = _context.OrderLines.AsQueryable();

            // Apply date filters if provided
            if (fromDate.HasValue || toDate.HasValue)
            {
                // Join with Orders to get order date
                query = query.Where(ol => ol.Order != null);
                
                if (fromDate.HasValue)
                    query = query.Where(ol => ol.Order!.OrderDate >= fromDate.Value);
                
                if (toDate.HasValue)
                    query = query.Where(ol => ol.Order!.OrderDate <= toDate.Value);
            }

            object analysis;

            // Group by the specified dimension
            switch (dimensionLevel.ToUpper())
            {
                case "R1":
                    analysis = await query
                        .Where(ol => ol.R1.HasValue)
                        .GroupBy(ol => ol.R1)
                        .Select(g => new
                        {
                            DimensionValue = g.Key,
                            TotalAmount = g.Sum(ol => ol.Amount),
                            TotalQuantity = g.Sum(ol => ol.NoRegistered),
                            OrderCount = g.Count(),
                            UniqueOrders = g.Select(ol => ol.OrderNumber).Distinct().Count()
                        })
                        .OrderByDescending(x => x.TotalAmount)
                        .Take(limit)
                        .ToListAsync();
                    break;
                case "R2":
                    analysis = await query
                        .Where(ol => ol.R2.HasValue)
                        .GroupBy(ol => ol.R2)
                        .Select(g => new
                        {
                            DimensionValue = g.Key,
                            TotalAmount = g.Sum(ol => ol.Amount),
                            TotalQuantity = g.Sum(ol => ol.NoRegistered),
                            OrderCount = g.Count(),
                            UniqueOrders = g.Select(ol => ol.OrderNumber).Distinct().Count()
                        })
                        .OrderByDescending(x => x.TotalAmount)
                        .Take(limit)
                        .ToListAsync();
                    break;
                case "R3":
                    analysis = await query
                        .Where(ol => ol.R3.HasValue)
                        .GroupBy(ol => ol.R3)
                        .Select(g => new
                        {
                            DimensionValue = g.Key,
                            TotalAmount = g.Sum(ol => ol.Amount),
                            TotalQuantity = g.Sum(ol => ol.NoRegistered),
                            OrderCount = g.Count(),
                            UniqueOrders = g.Select(ol => ol.OrderNumber).Distinct().Count()
                        })
                        .OrderByDescending(x => x.TotalAmount)
                        .Take(limit)
                        .ToListAsync();
                    break;
                case "R7":
                    analysis = await query
                        .Where(ol => !string.IsNullOrEmpty(ol.R7))
                        .GroupBy(ol => ol.R7)
                        .Select(g => new
                        {
                            DimensionValue = g.Key,
                            TotalAmount = g.Sum(ol => ol.Amount),
                            TotalQuantity = g.Sum(ol => ol.NoRegistered),
                            OrderCount = g.Count(),
                            UniqueOrders = g.Select(ol => ol.OrderNumber).Distinct().Count()
                        })
                        .OrderByDescending(x => x.TotalAmount)
                        .Take(limit)
                        .ToListAsync();
                    break;
                // Add other dimensions as needed
                default:
                    return JsonSerializer.Serialize(new 
                    { 
                        error = "Dimension analysis not implemented", 
                        message = $"Analysis for dimension {dimensionLevel} is not yet implemented" 
                    });
            }

            return JsonSerializer.Serialize(new
            {
                dimensionLevel = dimensionLevel.ToUpper(),
                analysis = analysis,
                filters = new
                {
                    fromDate = fromDate,
                    toDate = toDate
                },
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // CUSTOMER AND SUPPLIER TRANSACTION TOOLS

    /// <summary>
    /// Gets customer transactions with filtering
    /// </summary>
    /// <param name="customerNumber">Optional customer number filter</param>
    /// <param name="transactionType">Optional transaction type filter (Invoice, Payment, Credit Note, etc.)</param>
    /// <param name="unpaidOnly">Only return unpaid transactions (default: false)</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="limit">Maximum number of transactions to return (default: 100)</param>
    /// <param name="offset">Number of transactions to skip (default: 0)</param>
    /// <returns>JSON array of customer transaction objects</returns>
    [McpServerTool]
    [Description("Gets customer transactions with optional filtering by customer, transaction type, payment status, and date range.")]
    public async Task<string> GetCustomerTransactions(
        [Description("Optional customer number filter")]
        int? customerNumber = null,
        [Description("Optional transaction type filter (Invoice, Payment, Credit Note, etc.)")]
        string? transactionType = null,
        [Description("Only return unpaid transactions (default: false)")]
        bool unpaidOnly = false,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Maximum number of transactions to return (default: 100, max: 200)")]
        int limit = 100,
        [Description("Number of transactions to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 200) limit = 200;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

        var query = _context.CustomerTransactions.AsQueryable();

        if (customerNumber.HasValue)
            query = query.Where(t => t.CustomerNumber == customerNumber.Value);

        if (!string.IsNullOrEmpty(transactionType))
            query = query.Where(t => t.Credit.ToString() == transactionType);

        if (unpaidOnly)
            query = query.Where(t => (t.RemainingAmount == 0) != true && t.RemainingAmount > 0);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.VoucherDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.VoucherDate <= toDate.Value);
        }

        var transactions = await query
            .Include(t => t.Customer)
            .OrderByDescending(t => t.VoucherDate)
            .ThenByDescending(t => t.JournalNumber)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        var totalCount = await query.CountAsync();

        // Calculate totals
        var totalAmount = await query.SumAsync(t => t.Amount);
        var totalOutstanding = await query.SumAsync(t => t.RemainingAmount);

            return JsonSerializer.Serialize(new
            {
                transactions = transactions,
                count = transactions.Count,
                total = totalCount,
                totals = new
                {
                    amount = totalAmount,
                    outstanding = totalOutstanding
                },
                filters = new
                {
                    customerNumber = customerNumber,
                    transactionType = transactionType,
                    unpaidOnly = unpaidOnly,
                    fromDate = fromDate,
                    toDate = toDate
                },
                offset = offset,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets supplier transactions with filtering
    /// </summary>
    /// <param name="supplierNumber">Optional supplier number filter</param>
    /// <param name="transactionType">Optional transaction type filter (Invoice, Payment, Credit Note, etc.)</param>
    /// <param name="unpaidOnly">Only return unpaid transactions (default: false)</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="limit">Maximum number of transactions to return (default: 100)</param>
    /// <param name="offset">Number of transactions to skip (default: 0)</param>
    /// <returns>JSON array of supplier transaction objects</returns>
    [McpServerTool]
    [Description("Gets supplier transactions with optional filtering by supplier, transaction type, payment status, and date range.")]
    public async Task<string> GetSupplierTransactions(
        [Description("Optional supplier number filter")]
        int? supplierNumber = null,
        [Description("Optional transaction type filter (Invoice, Payment, Credit Note, etc.)")]
        string? transactionType = null,
        [Description("Only return unpaid transactions (default: false)")]
        bool unpaidOnly = false,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Maximum number of transactions to return (default: 100, max: 200)")]
        int limit = 100,
        [Description("Number of transactions to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 200) limit = 200;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

        var query = _context.SupplierTransactions.AsQueryable();

        if (supplierNumber.HasValue)
            query = query.Where(t => t.SupplierNumber == supplierNumber.Value);

        if (!string.IsNullOrEmpty(transactionType))
            query = query.Where(t => t.Credit.ToString() == transactionType);

        if (unpaidOnly)
            query = query.Where(t => (t.RemainingAmount == 0) != true && t.RemainingAmount > 0);

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.VoucherDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(t => t.VoucherDate <= toDate.Value);
        }

        var transactions = await query
            .Include(t => t.Supplier)
            .OrderByDescending(t => t.VoucherDate)
            .ThenByDescending(t => t.JournalNumber)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        var totalCount = await query.CountAsync();

        // Calculate totals
        var totalAmount = await query.SumAsync(t => t.Amount);
        var totalOutstanding = await query.SumAsync(t => t.RemainingAmount);

            return JsonSerializer.Serialize(new
            {
                transactions = transactions,
                count = transactions.Count,
                total = totalCount,
                totals = new
                {
                    amount = totalAmount,
                    outstanding = totalOutstanding
                },
                filters = new
                {
                    supplierNumber = supplierNumber,
                    transactionType = transactionType,
                    unpaidOnly = unpaidOnly,
                    fromDate = fromDate,
                    toDate = toDate
                },
                offset = offset,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Analyzes dimension usage across order lines using raw SQL to avoid mapping issues
    /// </summary>
    /// <param name="dimensionLevel">Dimension level to analyze (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)</param>
    /// <param name="limit">Maximum number of results to return (default: 20)</param>
    /// <returns>JSON array with dimension usage analysis</returns>
    [McpServerTool]
    [Description("Analyzes which financial dimensions are actually used in transactions, using raw SQL to avoid column mapping issues.")]
    public async Task<string> AnalyzeDimensionUsage(
        [Description("Dimension level to analyze (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)")]
        string dimensionLevel,
        [Description("Maximum number of results to return (default: 20, max: 50)")]
        int limit = 20)
    {
        try
        {
            // Validate dimension level
            var validDimensions = new[] { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12" };
            if (!validDimensions.Contains(dimensionLevel.ToUpper()))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Invalid dimension level", 
                    message = $"Dimension level must be one of: {string.Join(", ", validDimensions)}" 
                });
            }

            if (limit > 50) limit = 50;
            if (limit < 1) limit = 1;

            var dimension = dimensionLevel.ToUpper();
            
            // Use raw SQL with actual column names to avoid Entity Framework mapping issues
            var sqlQuery = $@"
                SELECT 
                    {dimension} as DimensionValue,
                    COUNT(*) as TransactionCount,
                    COUNT(DISTINCT OrdNo) as UniqueOrders,
                    ISNULL(SUM(Amt), 0) as TotalAmount,
                    ISNULL(SUM(Qty), 0) as TotalQuantity
                FROM OrdLn 
                WHERE {dimension} IS NOT NULL 
                GROUP BY {dimension}
                ORDER BY COUNT(*) DESC, TotalAmount DESC
                OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sqlQuery;
            
            await _context.Database.OpenConnectionAsync();
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<object>();
            
            while (await reader.ReadAsync())
            {
                var result = new
                {
                    DimensionValue = reader.IsDBNull(0) ? null : reader.GetValue(0),
                    TransactionCount = reader.GetInt32(1),
                    UniqueOrders = reader.GetInt32(2),
                    TotalAmount = reader.GetDecimal(3),
                    TotalQuantity = reader.GetDecimal(4)
                };
                results.Add(result);
            }

            // Also get overall usage statistics
            var statsQuery = $@"
                SELECT 
                    COUNT(*) as TotalTransactions,
                    COUNT(CASE WHEN {dimension} IS NOT NULL THEN 1 END) as TransactionsWithDimension,
                    COUNT(DISTINCT {dimension}) as UniqueDimensionValues,
                    CAST(COUNT(CASE WHEN {dimension} IS NOT NULL THEN 1 END) * 100.0 / COUNT(*) as DECIMAL(5,2)) as UsagePercentage
                FROM OrdLn";

            command.CommandText = statsQuery;
            using var statsReader = await command.ExecuteReaderAsync();
            
            object? usageStats = null;
            if (await statsReader.ReadAsync())
            {
                usageStats = new
                {
                    TotalTransactions = statsReader.GetInt32(0),
                    TransactionsWithDimension = statsReader.GetInt32(1),
                    UniqueDimensionValues = statsReader.GetInt32(2),
                    UsagePercentage = statsReader.GetDecimal(3)
                };
            }

            return JsonSerializer.Serialize(new
            {
                dimensionLevel = dimension,
                usageStats = usageStats,
                dimensionValues = results,
                count = results.Count,
                limit = limit
            }, new JsonSerializerOptions 
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true 
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ENHANCED RELATIONSHIP ANALYSIS TOOLS

    /// <summary>
    /// Gets orders with full actor and financial dimension details
    /// </summary>
    /// <param name="orderNumber">Optional order number to search for</param>
    /// <param name="deliveryActor">Optional delivery actor number filter</param>
    /// <param name="liableActor">Optional liable actor number filter</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (optional)</param>
    /// <param name="toDate">End date in YYYYMMDD format (optional)</param>
    /// <param name="includeFinancialDimensions">Include financial dimension details (default: true)</param>
    /// <param name="limit">Maximum number of orders to return (default: 20)</param>
    /// <param name="offset">Number of orders to skip (default: 0)</param>
    /// <returns>JSON array of order objects with full relationship details</returns>
    [McpServerTool]
    [Description("Gets orders with comprehensive actor and financial dimension details, showing full relationships across tables.")]
    public async Task<string> GetOrdersWithFullDetails(
        [Description("Optional order number to search for")]
        int? orderNumber = null,
        [Description("Optional delivery actor number filter")]
        int? deliveryActor = null,
        [Description("Optional liable actor number filter")]
        int? liableActor = null,
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Include financial dimension details (default: true)")]
        bool includeFinancialDimensions = true,
        [Description("Maximum number of orders to return (default: 20, max: 50)")]
        int limit = 20,
        [Description("Number of orders to skip for pagination (default: 0)")]
        int offset = 0)
    {
        try
        {
            // Limit validation
            if (limit > 50) limit = 50;
            if (limit < 1) limit = 1;
            if (offset < 0) offset = 0;

            var query = _context.Orders.AsQueryable();

            // Apply filters
            if (orderNumber.HasValue)
                query = query.Where(o => o.OrderNumber == orderNumber.Value);

            if (deliveryActor.HasValue)
                query = query.Where(o => o.DeliveryActorNumber == deliveryActor.Value);

            if (liableActor.HasValue)
                query = query.Where(o => o.LiableActorNumber == liableActor.Value);

            if (fromDate.HasValue)
                query = query.Where(o => o.OrderDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.OrderDate <= toDate.Value);

            var orders = await query
                // .Include(o => o.DeliveryActor)  // Temporarily disabled
                // .Include(o => o.LiableActor)    // Temporarily disabled  
                // .Include(o => o.ShipActor)      // Temporarily disabled
                .Include(o => o.OrderLines)
                    .ThenInclude(ol => ol.Product)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderNumber)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            // Transform to result format avoiding expression tree limitations
            var result = orders.Select(o => new
            {
                Order = new
                {
                    o.OrderNumber,
                    o.OrderType,
                    o.OrderDate,
                    DeliveryActorNumber = o.DeliveryActorNumber,
                    LiableActorNumber = o.LiableActorNumber
                },
                // Actor information temporarily disabled due to navigation property mapping issues
                DeliveryActor = (object?)null,
                LiableActor = (object?)null,
                ShipActor = (object?)null,
                OrderLines = o.OrderLines.Select(ol => new
                {
                    ol.LineNumber,
                    ol.ProductNumber,
                    ProductDescription = ol.Product != null ? ol.Product.Description : null,
                    ol.NoRegistered,
                    ol.NoInvoiced,
                    ol.Price,
                    ol.Amount,
                    ol.VatAmount,
                    ol.Description,
                    ol.DeliveryDate,
                    FinancialDimensions = includeFinancialDimensions ? new
                    {
                        R1 = ol.R1,
                        R2 = ol.R2,
                        R3 = ol.R3,
                        R4 = ol.R4,
                        R5 = ol.R5,
                        R6 = ol.R6,
                        R7 = ol.R7,
                        R8 = ol.R8,
                        R9 = ol.R9,
                        R10 = ol.R10,
                        R11 = ol.R11,
                        R12 = ol.R12
                    } : null
                }).ToList(),
                Totals = new
                {
                    LinesCount = o.OrderLines.Count,
                    TotalAmount = o.OrderLines.Sum(ol => ol.Amount),
                    TotalQuantity = o.OrderLines.Sum(ol => ol.NoRegistered),
                    TotalDeliveredQuantity = o.OrderLines.Sum(ol => ol.NoInvoiced)
                }
            }).ToList();

            var totalCount = await query.CountAsync();

            return JsonSerializer.Serialize(new
            {
                orders = result,
                count = result.Count,
                total = totalCount,
                includeFinancialDimensions = includeFinancialDimensions,
                filters = new
                {
                    orderNumber = orderNumber,
                    deliveryActor = deliveryActor,
                    liableActor = liableActor,
                    fromDate = fromDate,
                    toDate = toDate
                },
                offset = offset,
                limit = limit
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets customer analysis with transaction history and order summary
    /// </summary>
    /// <param name="customerNumber">Optional customer number filter</param>
    /// <param name="actorId">Optional actor ID filter</param>
    /// <param name="fromDate">Start date for analysis (YYYYMMDD format)</param>
    /// <param name="toDate">End date for analysis (YYYYMMDD format)</param>
    /// <param name="includeTransactions">Include transaction details (default: true)</param>
    /// <param name="includeOrders">Include order details (default: true)</param>
    /// <param name="limit">Maximum number of customers to analyze (default: 20)</param>
    /// <returns>JSON array with comprehensive customer analysis</returns>
    [McpServerTool]
    [Description("Provides comprehensive customer analysis including transaction history, order summary, and financial metrics.")]
    public async Task<string> GetCustomerAnalysis(
        [Description("Optional customer number filter")]
        string? customerNumber = null,
        [Description("Optional actor ID filter")]
        int? actorId = null,
        [Description("Start date for analysis in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date for analysis in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Include transaction details (default: true)")]
        bool includeTransactions = true,
        [Description("Include order details (default: true)")]
        bool includeOrders = true,
        [Description("Maximum number of customers to analyze (default: 20, max: 50)")]
        int limit = 20)
    {
        try
        {
            // Limit validation
            if (limit > 50) limit = 50;
            if (limit < 1) limit = 1;

            // Build customer analysis using raw SQL for flexibility
            var sql = @"
                WITH CustomerSummary AS (
                    SELECT 
                        ct.CustNo,
                        COUNT(ct.TrId) as TransactionCount,
                        SUM(CASE WHEN ct.TrTp = 'Invoice' THEN ct.Amt ELSE 0 END) as TotalInvoiced,
                        SUM(CASE WHEN ct.TrTp = 'Payment' THEN ABS(ct.Amt) ELSE 0 END) as TotalPaid,
                        SUM(ct.OutAmt) as TotalOutstanding,
                        MIN(ct.TrDt) as FirstTransactionDate,
                        MAX(ct.TrDt) as LastTransactionDate
                    FROM custtr ct";

            if (fromDate.HasValue || toDate.HasValue)
            {
                sql += " WHERE ";
                var conditions = new List<string>();
                
                if (fromDate.HasValue)
                    conditions.Add($"ct.TrDt >= '{DateTime.ParseExact(fromDate.Value.ToString(), "yyyyMMdd", null):yyyy-MM-dd}'");
                
                if (toDate.HasValue)
                    conditions.Add($"ct.TrDt <= '{DateTime.ParseExact(toDate.Value.ToString(), "yyyyMMdd", null):yyyy-MM-dd}'");
                
                sql += string.Join(" AND ", conditions);
            }

            sql += @"
                    GROUP BY ct.CustNo
                ),
                OrderSummary AS (
                    SELECT 
                        CAST(o.LiaActNo as VARCHAR(50)) as CustomerNumber,
                        COUNT(o.OrdNo) as OrderCount,
                        SUM(ol.Amt) as TotalOrderAmount,
                        COUNT(ol.LnNo) as TotalOrderLines
                    FROM Ord o
                    INNER JOIN OrdLn ol ON o.OrdNo = ol.OrdNo";

            if (fromDate.HasValue || toDate.HasValue)
            {
                sql += " WHERE ";
                var conditions = new List<string>();
                
                if (fromDate.HasValue)
                    conditions.Add($"o.OrdDt >= {fromDate.Value}");
                
                if (toDate.HasValue)
                    conditions.Add($"o.OrdDt <= {toDate.Value}");
                
                sql += string.Join(" AND ", conditions);
            }

            sql += @"
                    GROUP BY o.LiaActNo
                )
                SELECT 
                    cs.CustNo,
                    a.ActNo,
                    a.Nm as Name,
                    a.Ad1 as Address1,
                    cs.TransactionCount,
                    cs.TotalInvoiced,
                    cs.TotalPaid,
                    cs.TotalOutstanding,
                    cs.FirstTransactionDate,
                    cs.LastTransactionDate,
                    ISNULL(os.OrderCount, 0) as OrderCount,
                    ISNULL(os.TotalOrderAmount, 0) as TotalOrderAmount,
                    ISNULL(os.TotalOrderLines, 0) as TotalOrderLines
                FROM CustomerSummary cs
                LEFT JOIN Actor a ON TRY_CAST(cs.CustNo as INT) = a.ActNo
                LEFT JOIN OrderSummary os ON cs.CustNo = os.CustomerNumber";

            var conditions2 = new List<string>();
            if (!string.IsNullOrEmpty(customerNumber))
                conditions2.Add($"cs.CustNo = '{customerNumber}'");
            
            if (actorId.HasValue)
                conditions2.Add($"a.ActNo = {actorId.Value}");

            if (conditions2.Any())
                sql += " WHERE " + string.Join(" AND ", conditions2);

            sql += $" ORDER BY cs.TotalInvoiced DESC OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            
            await _context.Database.OpenConnectionAsync();
            
            using var reader = await command.ExecuteReaderAsync();
            var customerAnalysis = new List<Dictionary<string, object?>>();
            
            while (await reader.ReadAsync())
            {
                var customer = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    customer[fieldName] = value;
                }
                customerAnalysis.Add(customer);
            }

            return JsonSerializer.Serialize(new
            {
                customerAnalysis = customerAnalysis,
                count = customerAnalysis.Count,
                parameters = new
                {
                    customerNumber = customerNumber,
                    actorId = actorId,
                    fromDate = fromDate,
                    toDate = toDate,
                    includeTransactions = includeTransactions,
                    includeOrders = includeOrders
                },
                limit = limit
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Gets financial dimension usage analysis across all order lines
    /// </summary>
    /// <param name="dimensionLevel">Dimension level to analyze (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)</param>
    /// <param name="includeHierarchy">Include parent-child relationships for dimensions that support it (default: true)</param>
    /// <param name="fromDate">Start date for analysis (YYYYMMDD format)</param>
    /// <param name="toDate">End date for analysis (YYYYMMDD format)</param>
    /// <param name="limit">Maximum number of dimension values to return (default: 50)</param>
    /// <returns>JSON object with comprehensive dimension analysis</returns>
    [McpServerTool]
    [Description("Provides comprehensive financial dimension usage analysis with totals, hierarchy, and usage statistics.")]
    public async Task<string> GetComprehensiveDimensionAnalysis(
        [Description("Dimension level to analyze (R1, R2, R3, R4, R5, R6, R7, R8, R9, R10, R11, R12)")]
        string dimensionLevel,
        [Description("Include parent-child relationships for dimensions that support it (default: true)")]
        bool includeHierarchy = true,
        [Description("Start date for analysis in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date for analysis in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Maximum number of dimension values to return (default: 50, max: 100)")]
        int limit = 50)
    {
        try
        {
            // Validate dimension level
            var validDimensions = new[] { "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R11", "R12" };
            if (!validDimensions.Contains(dimensionLevel.ToUpper()))
            {
                return JsonSerializer.Serialize(new 
                { 
                    error = "Invalid dimension level", 
                    message = $"Dimension level must be one of: {string.Join(", ", validDimensions)}" 
                });
            }

            // Limit validation
            if (limit > 100) limit = 100;
            if (limit < 1) limit = 1;

            var dimension = dimensionLevel.ToUpper();
            var isStringDimension = new[] { "R7", "R8", "R9", "R10", "R11", "R12" }.Contains(dimension);

            // Build comprehensive analysis query
            var sql = $@"
                WITH DimensionUsage AS (
                    SELECT 
                        ol.{dimension} as DimensionValue,
                        COUNT(*) as UsageCount,
                        SUM(ol.Amt) as TotalAmount,
                        SUM(ol.Qty) as TotalQuantity,
                        COUNT(DISTINCT ol.OrdNo) as UniqueOrders,
                        COUNT(DISTINCT ol.ProdNo) as UniqueProducts,
                        MIN(o.OrdDt) as FirstUsageDate,
                        MAX(o.OrdDt) as LastUsageDate
                    FROM OrdLn ol
                    INNER JOIN Ord o ON ol.OrdNo = o.OrdNo
                    WHERE ol.{dimension} IS NOT NULL";

            if (fromDate.HasValue)
                sql += $" AND o.OrdDt >= {fromDate.Value}";
            
            if (toDate.HasValue)
                sql += $" AND o.OrdDt <= {toDate.Value}";

            sql += $@"
                    GROUP BY ol.{dimension}
                ),
                DimensionDetails AS (
                    SELECT 
                        d.{dimension} as DimensionKey,
                        d.Nm as Name,
                        d.Desc as Description";

            if (includeHierarchy && new[] { "R7", "R8", "R9", "R10", "R11", "R12" }.Contains(dimension))
            {
                sql += $@",
                        d.Par{dimension} as ParentKey,
                        pd.Nm as ParentName";
            }

            sql += $@"
                    FROM {dimension} d";

            if (includeHierarchy && new[] { "R7", "R8", "R9", "R10", "R11", "R12" }.Contains(dimension))
            {
                sql += $@"
                    LEFT JOIN {dimension} pd ON d.Par{dimension} = pd.{dimension}";
            }

            sql += $@"
                )
                SELECT 
                    du.DimensionValue,
                    dd.Name,
                    dd.Description,";

            if (includeHierarchy && new[] { "R7", "R8", "R9", "R10", "R11", "R12" }.Contains(dimension))
            {
                sql += @"
                    dd.ParentKey,
                    dd.ParentName,";
            }

            sql += $@"
                    du.UsageCount,
                    du.TotalAmount,
                    du.TotalQuantity,
                    du.UniqueOrders,
                    du.UniqueProducts,
                    du.FirstUsageDate,
                    du.LastUsageDate
                FROM DimensionUsage du
                LEFT JOIN DimensionDetails dd ON du.DimensionValue = dd.DimensionKey
                ORDER BY du.TotalAmount DESC
                OFFSET 0 ROWS FETCH NEXT {limit} ROWS ONLY";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            
            await _context.Database.OpenConnectionAsync();
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<Dictionary<string, object?>>();
            
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var fieldName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[fieldName] = value;
                }
                results.Add(row);
            }

            // Get overall statistics
            command.CommandText = $@"
                SELECT 
                    COUNT(*) as TotalOrderLines,
                    COUNT(CASE WHEN {dimension} IS NOT NULL THEN 1 END) as OrderLinesWithDimension,
                    COUNT(DISTINCT {dimension}) as UniqueDimensionValues,
                    SUM(Amt) as TotalAmountAllLines,
                    SUM(CASE WHEN {dimension} IS NOT NULL THEN Amt ELSE 0 END) as TotalAmountWithDimension
                FROM OrdLn ol
                INNER JOIN Ord o ON ol.OrdNo = o.OrdNo";

            var whereConditions = new List<string>();
            if (fromDate.HasValue)
                whereConditions.Add($"o.OrdDt >= {fromDate.Value}");
            if (toDate.HasValue)
                whereConditions.Add($"o.OrdDt <= {toDate.Value}");

            if (whereConditions.Any())
                command.CommandText += " WHERE " + string.Join(" AND ", whereConditions);

            using var statsReader = await command.ExecuteReaderAsync();
            
            object? overallStats = null;
            if (await statsReader.ReadAsync())
            {
                var totalLines = statsReader.GetInt32(0);
                var linesWithDimension = statsReader.GetInt32(1);
                var uniqueValues = statsReader.GetInt32(2);
                var totalAmount = statsReader.IsDBNull(3) ? 0m : statsReader.GetDecimal(3);
                var amountWithDimension = statsReader.IsDBNull(4) ? 0m : statsReader.GetDecimal(4);

                overallStats = new
                {
                    TotalOrderLines = totalLines,
                    OrderLinesWithDimension = linesWithDimension,
                    UniqueDimensionValues = uniqueValues,
                    TotalAmountAllLines = totalAmount,
                    TotalAmountWithDimension = amountWithDimension,
                    UsagePercentage = totalLines > 0 ? Math.Round((decimal)linesWithDimension / totalLines * 100, 2) : 0,
                    AmountCoveragePercentage = totalAmount > 0 ? Math.Round(amountWithDimension / totalAmount * 100, 2) : 0
                };
            }

            return JsonSerializer.Serialize(new
            {
                dimensionLevel = dimension,
                overallStatistics = overallStats,
                dimensionAnalysis = results,
                count = results.Count,
                parameters = new
                {
                    includeHierarchy = includeHierarchy,
                    fromDate = fromDate,
                    toDate = toDate
                },
                limit = limit
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ==================== FINANCIAL STATEMENTS & REPORTS ====================

    /// <summary>
    /// Generates a Profit & Loss statement for a specified period
    /// </summary>
    /// <param name="fromDate">Start date in YYYYMMDD format (e.g., 20240101)</param>
    /// <param name="toDate">End date in YYYYMMDD format (e.g., 20241231)</param>
    /// <param name="includeComparison">Include previous period comparison (default: true)</param>
    /// <returns>JSON object with profit and loss statement</returns>
    [McpServerTool]
    [Description("Generates a Profit & Loss statement for a specified period with optional previous period comparison.")]
    public async Task<string> GetProfitLossStatement(
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("Include previous period comparison (default: true)")]
        bool includeComparison = true)
    {
        try
        {
            var sql = @"
                SELECT 
                    a.AcNo AS AccountNumber,
                    a.Nm AS AccountName,
                    a.AcGr AS AccountGroup,
                    CASE 
                        WHEN a.AcNo BETWEEN 3000 AND 3999 THEN 'Revenue'
                        WHEN a.AcNo BETWEEN 4000 AND 4999 THEN 'Cost of Goods Sold'
                        WHEN a.AcNo BETWEEN 5000 AND 5999 THEN 'Operating Expenses'
                        WHEN a.AcNo BETWEEN 6000 AND 6999 THEN 'Financial Items'
                        WHEN a.AcNo BETWEEN 7000 AND 7999 THEN 'Other Income/Expense'
                        WHEN a.AcNo BETWEEN 8000 AND 8999 THEN 'Extraordinary Items'
                        ELSE 'Other'
                    END AS AccountType,
                    COALESCE(SUM(CASE WHEN at.Cr = 0 THEN at.AcAm ELSE -at.AcAm END), 0) AS Amount
                FROM Ac a
                LEFT JOIN AcTr at ON a.AcNo = at.AcNo 
                    AND (@fromDate IS NULL OR at.VoDt >= @fromDate)
                    AND (@toDate IS NULL OR at.VoDt <= @toDate)
                WHERE a.AcNo BETWEEN 3000 AND 8999  -- P&L accounts only
                    AND a.Nm != ' '  -- Exclude empty account names
                GROUP BY a.AcNo, a.Nm, a.AcGr
                HAVING ABS(COALESCE(SUM(CASE WHEN at.Cr = 0 THEN at.AcAm ELSE -at.AcAm END), 0)) > 0.01
                ORDER BY a.AcNo";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var fromDateParam = command.CreateParameter();
            fromDateParam.ParameterName = "@fromDate";
            fromDateParam.Value = (object?)fromDate ?? DBNull.Value;
            command.Parameters.Add(fromDateParam);

            var toDateParam = command.CreateParameter();
            toDateParam.ParameterName = "@toDate";
            toDateParam.Value = (object?)toDate ?? DBNull.Value;
            command.Parameters.Add(toDateParam);

            var results = new List<object>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    AccountNumber = reader.GetInt32(0),
                    AccountName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    AccountGroup = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    AccountType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Amount = reader.GetDecimal(4)
                });
            }

            // Calculate totals by category
            decimal revenue = 0, costOfGoodsSold = 0, operatingExpenses = 0, financialItems = 0, otherIncomeExpense = 0, extraordinaryItems = 0;
            
            foreach (var r in results)
            {
                var record = (dynamic)r;
                var accountType = record.AccountType?.ToString() ?? "";
                var amount = Convert.ToDecimal(record.Amount);
                
                switch (accountType)
                {
                    case "Revenue":
                        revenue += -amount; // Revenue is normally credit, so negate to show positive
                        break;
                    case "Cost of Goods Sold":
                        costOfGoodsSold += amount;
                        break;
                    case "Operating Expenses":
                        operatingExpenses += amount;
                        break;
                    case "Financial Items":
                        financialItems += amount;
                        break;
                    case "Other Income/Expense":
                        otherIncomeExpense += amount;
                        break;
                    case "Extraordinary Items":
                        extraordinaryItems += amount;
                        break;
                }
            }

            var totalExpenses = costOfGoodsSold + operatingExpenses + Math.Abs(financialItems) + Math.Abs(otherIncomeExpense) + Math.Abs(extraordinaryItems);
            var netIncome = revenue - totalExpenses;

            return JsonSerializer.Serialize(new
            {
                profitLossStatement = new
                {
                    period = new { fromDate = fromDate, toDate = toDate },
                    revenue = revenue,
                    costOfGoodsSold = costOfGoodsSold,
                    grossProfit = revenue - costOfGoodsSold,
                    operatingExpenses = operatingExpenses,
                    operatingIncome = revenue - costOfGoodsSold - operatingExpenses,
                    financialItems = financialItems,
                    otherIncomeExpense = otherIncomeExpense,
                    extraordinaryItems = extraordinaryItems,
                    totalExpenses = totalExpenses,
                    netIncome = netIncome,
                    accounts = results
                },
                summary = new
                {
                    totalAccounts = results.Count,
                    revenueAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Revenue"),
                    expenseAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString()?.Contains("Expense") == true),
                    breakdown = new
                    {
                        revenueAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Revenue"),
                        cogsAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Cost of Goods Sold"),
                        operatingExpenseAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Operating Expenses"),
                        financialAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Financial Items"),
                        otherAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Other Income/Expense"),
                        extraordinaryAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString() == "Extraordinary Items")
                    }
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Generates a Balance Sheet as of a specific date
    /// </summary>
    /// <param name="asOfDate">Date for balance sheet in YYYYMMDD format (default: current date)</param>
    /// <param name="includeZeroBalances">Include accounts with zero balances (default: false)</param>
    /// <returns>JSON object with balance sheet</returns>
    [McpServerTool]
    [Description("Generates a Balance Sheet as of a specific date showing assets, liabilities, and equity.")]
    public async Task<string> GetBalanceSheet(
        [Description("Date for balance sheet in YYYYMMDD format (default: current date)")]
        int? asOfDate = null,
        [Description("Include accounts with zero balances (default: false)")]
        bool includeZeroBalances = false)
    {
        try
        {
            var sql = @"
                SELECT 
                    a.AcNo AS AccountNumber,
                    a.Nm AS AccountName,
                    CASE 
                        WHEN a.AcNo BETWEEN 1000 AND 1399 THEN 'Fixed Assets'
                        WHEN a.AcNo BETWEEN 1400 AND 1999 THEN 'Current Assets'
                        WHEN a.AcNo BETWEEN 2000 AND 2099 THEN 'Equity'
                        WHEN a.AcNo BETWEEN 2100 AND 2199 THEN 'Untaxed Reserves'
                        WHEN a.AcNo BETWEEN 2200 AND 2399 THEN 'Long-term Liabilities'
                        WHEN a.AcNo BETWEEN 2400 AND 2999 THEN 'Current Liabilities'
                        ELSE 'Other'
                    END AS AccountType,
                    a.AcGr AS AccountGroup,
                    COALESCE(SUM(CASE WHEN at.Cr = 0 THEN at.AcAm ELSE -at.AcAm END), 0) AS Balance
                FROM Ac a
                LEFT JOIN AcTr at ON a.AcNo = at.AcNo 
                    AND (@asOfDate IS NULL OR at.VoDt <= @asOfDate)
                WHERE a.AcNo BETWEEN 1000 AND 2999  -- Balance sheet accounts only
                    AND a.Nm != ' '  -- Exclude empty account names
                GROUP BY a.AcNo, a.Nm, a.AcGr
                HAVING (@includeZeroBalances = 1 OR ABS(COALESCE(SUM(CASE WHEN at.Cr = 0 THEN at.AcAm ELSE -at.AcAm END), 0)) > 0.01)
                ORDER BY 
                    CASE 
                        WHEN a.AcNo BETWEEN 1000 AND 1399 THEN 1  -- Fixed Assets
                        WHEN a.AcNo BETWEEN 1400 AND 1999 THEN 2  -- Current Assets
                        WHEN a.AcNo BETWEEN 2000 AND 2099 THEN 3  -- Equity
                        WHEN a.AcNo BETWEEN 2100 AND 2199 THEN 4  -- Untaxed Reserves
                        WHEN a.AcNo BETWEEN 2200 AND 2399 THEN 5  -- Long-term Liabilities
                        WHEN a.AcNo BETWEEN 2400 AND 2999 THEN 6  -- Current Liabilities
                        ELSE 7
                    END, a.AcNo";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var asOfDateParam = command.CreateParameter();
            asOfDateParam.ParameterName = "@asOfDate";
            asOfDateParam.Value = (object?)asOfDate ?? DBNull.Value;
            command.Parameters.Add(asOfDateParam);

            var includeZeroParam = command.CreateParameter();
            includeZeroParam.ParameterName = "@includeZeroBalances";
            includeZeroParam.Value = includeZeroBalances;
            command.Parameters.Add(includeZeroParam);

            var results = new List<object>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    AccountNumber = reader.GetInt32(0),
                    AccountName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    AccountType = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    AccountGroup = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Balance = reader.GetDecimal(4)
                });
            }

            // Calculate totals by category
            var assets = results.Where(r => ((dynamic)r).AccountType?.ToString()?.Contains("Asset") == true)
                               .Sum(r => (decimal)((dynamic)r).Balance);
            
            var liabilities = results.Where(r => ((dynamic)r).AccountType?.ToString()?.Contains("Liability") == true)
                                    .Sum(r => (decimal)((dynamic)r).Balance);
            
            var equity = results.Where(r => ((dynamic)r).AccountType?.ToString()?.Contains("Equity") == true)
                               .Sum(r => (decimal)((dynamic)r).Balance);

            return JsonSerializer.Serialize(new
            {
                balanceSheet = new
                {
                    asOfDate = asOfDate,
                    assets = new
                    {
                        total = assets,
                        accounts = results.Where(r => ((dynamic)r).AccountType?.ToString()?.Contains("Asset") == true)
                    },
                    liabilities = new
                    {
                        total = liabilities,
                        accounts = results.Where(r => ((dynamic)r).AccountType?.ToString()?.Contains("Liability") == true)
                    },
                    equity = new
                    {
                        total = equity,
                        accounts = results.Where(r => ((dynamic)r).AccountType?.ToString()?.Contains("Equity") == true)
                    },
                    balanceCheck = assets - liabilities - equity,
                    isBalanced = Math.Abs(assets - liabilities - equity) < 0.01m
                },
                summary = new
                {
                    totalAccounts = results.Count,
                    assetAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString()?.Contains("Asset") == true),
                    liabilityAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString()?.Contains("Liability") == true),
                    equityAccounts = results.Count(r => ((dynamic)r).AccountType?.ToString()?.Contains("Equity") == true)
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    /// <summary>
    /// Generates a Cash Flow Statement for a specified period
    /// </summary>
    /// <param name="fromDate">Start date in YYYYMMDD format (e.g., 20240101)</param>
    /// <param name="toDate">End date in YYYYMMDD format (e.g., 20241231)</param>
    /// <returns>JSON object with cash flow statement</returns>
    [McpServerTool]
    [Description("Generates a Cash Flow Statement showing operating, investing, and financing activities.")]
    public async Task<string> GetCashFlowStatement(
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null)
    {
        try
        {
            var sql = @"
                WITH CashFlowData AS (
                    SELECT 
                        a.AcNo,
                        a.Nm AS AccountName,
                        a.AcGr AS AccountGroup,
                        at.VoDt AS TransactionDate,
                        CASE WHEN at.Cr = 1 THEN -at.AcAm ELSE at.AcAm END AS NetAmount,
                        at.Txt AS Description,
                        CASE 
                            WHEN a.AcNo BETWEEN 1910 AND 1999 THEN 'Cash'  -- Cash and bank accounts
                            WHEN a.AcNo BETWEEN 3000 AND 3999 THEN 'Operating'  -- Revenue accounts
                            WHEN a.AcNo BETWEEN 4000 AND 8999 THEN 'Operating'  -- Expense accounts
                            WHEN a.AcNo BETWEEN 1100 AND 1399 THEN 'Investing'  -- Fixed assets
                            WHEN a.AcNo BETWEEN 2300 AND 2399 THEN 'Financing'  -- Long-term liabilities
                            WHEN a.AcGr LIKE '%skulder%' AND a.AcNo BETWEEN 2330 AND 2399 THEN 'Financing'  -- Loan accounts
                            ELSE 'Operating'
                        END AS CashFlowCategory
                    FROM Ac a
                    INNER JOIN AcTr at ON a.AcNo = at.AcNo
                    WHERE (@fromDate IS NULL OR at.VoDt >= @fromDate)
                        AND (@toDate IS NULL OR at.VoDt <= @toDate)
                        AND a.Nm != ' '  -- Exclude accounts with empty names
                )
                SELECT 
                    CashFlowCategory,
                    AcNo AS AccountNumber,
                    AccountName,
                    SUM(NetAmount) AS CashFlow
                FROM CashFlowData
                GROUP BY CashFlowCategory, AcNo, AccountName
                ORDER BY CashFlowCategory, AcNo";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var fromDateParam = command.CreateParameter();
            fromDateParam.ParameterName = "@fromDate";
            fromDateParam.Value = (object?)fromDate ?? DBNull.Value;
            command.Parameters.Add(fromDateParam);

            var toDateParam = command.CreateParameter();
            toDateParam.ParameterName = "@toDate";
            toDateParam.Value = (object?)toDate ?? DBNull.Value;
            command.Parameters.Add(toDateParam);

            var results = new List<object>();
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    Category = reader.GetString(0),
                    AccountNumber = reader.GetInt32(1),
                    AccountName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    CashFlow = reader.GetDecimal(3)
                });
            }

            // Calculate totals by category
            var operating = results.Where(r => ((dynamic)r).Category == "Operating").Sum(r => ((dynamic)r).CashFlow);
            var investing = results.Where(r => ((dynamic)r).Category == "Investing").Sum(r => ((dynamic)r).CashFlow);
            var financing = results.Where(r => ((dynamic)r).Category == "Financing").Sum(r => ((dynamic)r).CashFlow);
            var netCashFlow = operating + investing + financing;

            return JsonSerializer.Serialize(new
            {
                cashFlowStatement = new
                {
                    period = new { fromDate = fromDate, toDate = toDate },
                    operatingActivities = new
                    {
                        total = operating,
                        accounts = results.Where(r => ((dynamic)r).Category == "Operating")
                    },
                    investingActivities = new
                    {
                        total = investing,
                        accounts = results.Where(r => ((dynamic)r).Category == "Investing")
                    },
                    financingActivities = new
                    {
                        total = financing,
                        accounts = results.Where(r => ((dynamic)r).Category == "Financing")
                    },
                    netCashFlow = netCashFlow
                },
                summary = new
                {
                    totalTransactions = results.Count,
                    operatingCount = results.Count(r => ((dynamic)r).Category == "Operating"),
                    investingCount = results.Count(r => ((dynamic)r).Category == "Investing"),
                    financingCount = results.Count(r => ((dynamic)r).Category == "Financing")
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ==================== FINANCIAL RATIOS & KPIs ====================

    /// <summary>
    /// Calculates key financial ratios and KPIs
    /// </summary>
    /// <param name="asOfDate">Date for ratio calculation in YYYYMMDD format (default: current date)</param>
    /// <param name="includeIndustryBenchmarks">Include industry benchmark comparisons (default: false)</param>
    /// <returns>JSON object with financial ratios and KPIs</returns>
    [McpServerTool]
    [Description("Calculates key financial ratios and KPIs including liquidity, profitability, and efficiency ratios.")]
    public async Task<string> GetFinancialRatios(
        [Description("Date for ratio calculation in YYYYMMDD format (default: current date)")]
        int? asOfDate = null,
        [Description("Include industry benchmark comparisons (default: false)")]
        bool includeIndustryBenchmarks = false)
    {
        try
        {
            var sql = @"
                WITH FinancialData AS (
                    SELECT 
                        a.AcTp AS AccountType,
                        SUM(CASE WHEN a.AcTp IN ('Current Asset', 'Cash', 'Bank') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS CurrentAssets,
                        SUM(CASE WHEN a.AcTp IN ('Current Liability') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS CurrentLiabilities,
                        SUM(CASE WHEN a.AcTp IN ('Asset', 'Current Asset', 'Fixed Asset') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS TotalAssets,
                        SUM(CASE WHEN a.AcTp IN ('Liability', 'Current Liability', 'Long-term Liability') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS TotalLiabilities,
                        SUM(CASE WHEN a.AcTp IN ('Equity') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS TotalEquity,
                        SUM(CASE WHEN a.AcTp IN ('Revenue', 'Sales', 'Income') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS TotalRevenue,
                        SUM(CASE WHEN a.AcTp IN ('Expense', 'Cost', 'COGS') THEN COALESCE(a.Bal, 0) ELSE 0 END) AS TotalExpenses,
                        SUM(CASE WHEN a.AcNo LIKE '%Inventory%' OR a.AcNo LIKE '%Stock%' THEN COALESCE(a.Bal, 0) ELSE 0 END) AS Inventory
                    FROM ac a
                    GROUP BY a.AcTp
                ),
                AggregatedData AS (
                    SELECT 
                        SUM(CurrentAssets) AS CurrentAssets,
                        SUM(CurrentLiabilities) AS CurrentLiabilities,
                        SUM(TotalAssets) AS TotalAssets,
                        SUM(TotalLiabilities) AS TotalLiabilities,
                        SUM(TotalEquity) AS TotalEquity,
                        SUM(TotalRevenue) AS TotalRevenue,
                        SUM(TotalExpenses) AS TotalExpenses,
                        SUM(Inventory) AS Inventory
                    FROM FinancialData
                )
                SELECT * FROM AggregatedData";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;

            using var reader = await command.ExecuteReaderAsync();
            
            decimal currentAssets = 0, currentLiabilities = 0, totalAssets = 0, totalLiabilities = 0;
            decimal totalEquity = 0, totalRevenue = 0, totalExpenses = 0, inventory = 0;

            if (await reader.ReadAsync())
            {
                currentAssets = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                currentLiabilities = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                totalAssets = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                totalLiabilities = reader.IsDBNull(3) ? 0 : reader.GetDecimal(3);
                totalEquity = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                totalRevenue = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                totalExpenses = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                inventory = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
            }

            // Calculate financial ratios
            var currentRatio = currentLiabilities != 0 ? currentAssets / currentLiabilities : 0;
            var quickRatio = currentLiabilities != 0 ? (currentAssets - inventory) / currentLiabilities : 0;
            var debtToEquityRatio = totalEquity != 0 ? totalLiabilities / totalEquity : 0;
            var returnOnAssets = totalAssets != 0 ? (totalRevenue - totalExpenses) / totalAssets : 0;
            var returnOnEquity = totalEquity != 0 ? (totalRevenue - totalExpenses) / totalEquity : 0;
            var assetTurnover = totalAssets != 0 ? totalRevenue / totalAssets : 0;
            var profitMargin = totalRevenue != 0 ? (totalRevenue - totalExpenses) / totalRevenue : 0;

            var ratios = new
            {
                liquidityRatios = new
                {
                    currentRatio = Math.Round(currentRatio, 2),
                    quickRatio = Math.Round(quickRatio, 2),
                    workingCapital = currentAssets - currentLiabilities
                },
                leverageRatios = new
                {
                    debtToEquityRatio = Math.Round(debtToEquityRatio, 2),
                    debtRatio = totalAssets != 0 ? Math.Round(totalLiabilities / totalAssets, 2) : 0,
                    equityRatio = totalAssets != 0 ? Math.Round(totalEquity / totalAssets, 2) : 0
                },
                profitabilityRatios = new
                {
                    returnOnAssets = Math.Round(returnOnAssets * 100, 2),
                    returnOnEquity = Math.Round(returnOnEquity * 100, 2),
                    profitMargin = Math.Round(profitMargin * 100, 2),
                    grossProfitMargin = totalRevenue != 0 ? Math.Round((totalRevenue - totalExpenses) / totalRevenue * 100, 2) : 0
                },
                efficiencyRatios = new
                {
                    assetTurnover = Math.Round(assetTurnover, 2),
                    inventoryTurnover = inventory != 0 ? Math.Round(totalExpenses / inventory, 2) : 0
                }
            };

            return JsonSerializer.Serialize(new
            {
                financialRatios = ratios,
                asOfDate = asOfDate,
                underlyingData = new
                {
                    currentAssets = currentAssets,
                    currentLiabilities = currentLiabilities,
                    totalAssets = totalAssets,
                    totalLiabilities = totalLiabilities,
                    totalEquity = totalEquity,
                    totalRevenue = totalRevenue,
                    totalExpenses = totalExpenses,
                    inventory = inventory
                },
                interpretation = new
                {
                    currentRatioInterpretation = currentRatio > 2m ? "Strong" : currentRatio > 1m ? "Adequate" : "Weak",
                    debtLevelInterpretation = debtToEquityRatio < 0.5m ? "Conservative" : debtToEquityRatio < 1m ? "Moderate" : "High",
                    profitabilityInterpretation = profitMargin > 0.2m ? "Excellent" : profitMargin > 0.1m ? "Good" : profitMargin > 0 ? "Marginal" : "Loss"
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ==================== AGING REPORTS ====================

    /// <summary>
    /// Generates detailed aging report for customer receivables
    /// </summary>
    /// <param name="asOfDate">Date for aging calculation in YYYYMMDD format (default: current date)</param>
    /// <param name="includePaidInvoices">Include paid invoices in analysis (default: false)</param>
    /// <param name="limit">Maximum number of customers to return (default: 100, max: 500)</param>
    /// <returns>JSON object with customer aging analysis</returns>
    [McpServerTool]
    [Description("Generates detailed aging report for customer receivables showing overdue amounts by age brackets.")]
    public async Task<string> GetCustomerAgingReport(
        [Description("Date for aging calculation in YYYYMMDD format (default: current date)")]
        int? asOfDate = null,
        [Description("Include paid invoices in analysis (default: false)")]
        bool includePaidInvoices = false,
        [Description("Maximum number of customers to return (default: 100, max: 500)")]
        int limit = 100)
    {
        try
        {
            if (limit <= 0 || limit > 500) limit = 100;
            
            var cutoffDate = asOfDate.HasValue 
                ? DateTime.ParseExact(asOfDate.Value.ToString(), "yyyyMMdd", null)
                : DateTime.Today;

            var sql = @"
                WITH CustomerAging AS (
                    SELECT 
                        ct.CustNo AS CustomerNumber,
                        a.Nm + ' ' + COALESCE(a.Ad1, '') AS CustomerName,
                        ct.InvNo AS InvoiceNumber,
                        ct.InvDt AS InvoiceDate,
                        ct.DueDt AS DueDate,
                        ct.AmtDue AS AmountDue,
                        ct.AmtPaid AS AmountPaid,
                        (ct.AmtDue - COALESCE(ct.AmtPaid, 0)) AS Outstanding,
                        DATEDIFF(day, ct.DueDt, @asOfDate) AS DaysOverdue,
                        CASE 
                            WHEN DATEDIFF(day, ct.DueDt, @asOfDate) <= 0 THEN 'Current'
                            WHEN DATEDIFF(day, ct.DueDt, @asOfDate) <= 30 THEN '1-30 Days'
                            WHEN DATEDIFF(day, ct.DueDt, @asOfDate) <= 60 THEN '31-60 Days'
                            WHEN DATEDIFF(day, ct.DueDt, @asOfDate) <= 90 THEN '61-90 Days'
                            ELSE 'Over 90 Days'
                        END AS AgeBracket
                    FROM custtr ct
                    LEFT JOIN actor a ON ct.ActorId = a.ActorId
                    WHERE (ct.AmtDue - COALESCE(ct.AmtPaid, 0)) > 0.01
                        OR @includePaid = 1
                ),
                AgingSummary AS (
                    SELECT 
                        CustomerNumber,
                        CustomerName,
                        COUNT(*) AS InvoiceCount,
                        SUM(Outstanding) AS TotalOutstanding,
                        SUM(CASE WHEN AgeBracket = 'Current' THEN Outstanding ELSE 0 END) AS Current,
                        SUM(CASE WHEN AgeBracket = '1-30 Days' THEN Outstanding ELSE 0 END) AS Days1to30,
                        SUM(CASE WHEN AgeBracket = '31-60 Days' THEN Outstanding ELSE 0 END) AS Days31to60,
                        SUM(CASE WHEN AgeBracket = '61-90 Days' THEN Outstanding ELSE 0 END) AS Days61to90,
                        SUM(CASE WHEN AgeBracket = 'Over 90 Days' THEN Outstanding ELSE 0 END) AS Over90Days,
                        AVG(CAST(DaysOverdue AS FLOAT)) AS AvgDaysOverdue,
                        MAX(DaysOverdue) AS MaxDaysOverdue
                    FROM CustomerAging
                    GROUP BY CustomerNumber, CustomerName
                )
                SELECT TOP (@limit)
                    CustomerNumber,
                    CustomerName,
                    InvoiceCount,
                    TotalOutstanding,
                    Current,
                    Days1to30,
                    Days31to60,
                    Days61to90,
                    Over90Days,
                    AvgDaysOverdue,
                    MaxDaysOverdue
                FROM AgingSummary
                WHERE TotalOutstanding > 0.01
                ORDER BY TotalOutstanding DESC";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var asOfDateParam = command.CreateParameter();
            asOfDateParam.ParameterName = "@asOfDate";
            asOfDateParam.Value = cutoffDate;
            command.Parameters.Add(asOfDateParam);

            var includePaidParam = command.CreateParameter();
            includePaidParam.ParameterName = "@includePaid";
            includePaidParam.Value = includePaidInvoices;
            command.Parameters.Add(includePaidParam);

            var limitParam = command.CreateParameter();
            limitParam.ParameterName = "@limit";
            limitParam.Value = limit;
            command.Parameters.Add(limitParam);

            var results = new List<object>();
            using var reader = await command.ExecuteReaderAsync();
            
            decimal totalOutstanding = 0, totalCurrent = 0, total1to30 = 0, total31to60 = 0, total61to90 = 0, totalOver90 = 0;
            
            while (await reader.ReadAsync())
            {
                var outstanding = reader.GetDecimal(3);
                var current = reader.GetDecimal(4);
                var days1to30 = reader.GetDecimal(5);
                var days31to60 = reader.GetDecimal(6);
                var days61to90 = reader.GetDecimal(7);
                var over90 = reader.GetDecimal(8);

                totalOutstanding += outstanding;
                totalCurrent += current;
                total1to30 += days1to30;
                total31to60 += days31to60;
                total61to90 += days61to90;
                totalOver90 += over90;

                results.Add(new
                {
                    CustomerNumber = reader.IsDBNull(0) ? "" : reader.GetString(0),
                    CustomerName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    InvoiceCount = reader.GetInt32(2),
                    TotalOutstanding = outstanding,
                    Current = current,
                    Days1to30 = days1to30,
                    Days31to60 = days31to60,
                    Days61to90 = days61to90,
                    Over90Days = over90,
                    AvgDaysOverdue = reader.IsDBNull(9) ? 0 : Math.Round(reader.GetDouble(9), 1),
                    MaxDaysOverdue = reader.GetInt32(10)
                });
            }

            return JsonSerializer.Serialize(new
            {
                customerAgingReport = new
                {
                    asOfDate = cutoffDate.ToString("yyyy-MM-dd"),
                    summary = new
                    {
                        totalCustomers = results.Count,
                        totalOutstanding = totalOutstanding,
                        agingBreakdown = new
                        {
                            current = totalCurrent,
                            days1to30 = total1to30,
                            days31to60 = total31to60,
                            days61to90 = total61to90,
                            over90Days = totalOver90
                        },
                        percentageBreakdown = new
                        {
                            currentPercent = totalOutstanding > 0 ? Math.Round(totalCurrent / totalOutstanding * 100, 1) : 0,
                            days1to30Percent = totalOutstanding > 0 ? Math.Round(total1to30 / totalOutstanding * 100, 1) : 0,
                            days31to60Percent = totalOutstanding > 0 ? Math.Round(total31to60 / totalOutstanding * 100, 1) : 0,
                            days61to90Percent = totalOutstanding > 0 ? Math.Round(total61to90 / totalOutstanding * 100, 1) : 0,
                            over90Percent = totalOutstanding > 0 ? Math.Round(totalOver90 / totalOutstanding * 100, 1) : 0
                        }
                    },
                    customers = results
                },
                riskAnalysis = new
                {
                    highRiskCustomers = results.Count(r => ((dynamic)r).Over90Days > 1000),
                    overduePercentage = totalOutstanding > 0 ? Math.Round((totalOutstanding - totalCurrent) / totalOutstanding * 100, 1) : 0,
                    averageCollectionRisk = totalOver90 > totalOutstanding * 0.2m ? "High" : totalOver90 > totalOutstanding * 0.1m ? "Medium" : "Low"
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ==================== PROFITABILITY ANALYSIS ====================

    /// <summary>
    /// Analyzes profitability by customer, product, or financial dimension
    /// </summary>
    /// <param name="analysisType">Type of analysis: 'customer', 'product', or 'dimension' (default: 'customer')</param>
    /// <param name="fromDate">Start date in YYYYMMDD format (e.g., 20240101)</param>
    /// <param name="toDate">End date in YYYYMMDD format (e.g., 20241231)</param>
    /// <param name="dimensionLevel">For dimension analysis: R1, R2, R3, etc.</param>
    /// <param name="limit">Maximum number of results to return (default: 50, max: 200)</param>
    /// <returns>JSON object with profitability analysis</returns>
    [McpServerTool]
    [Description("Analyzes profitability by customer, product, or financial dimension showing revenue, costs, and margins.")]
    public async Task<string> GetProfitabilityAnalysis(
        [Description("Type of analysis: 'customer', 'product', or 'dimension' (default: 'customer')")]
        string analysisType = "customer",
        [Description("Start date in YYYYMMDD format (e.g., 20240101)")]
        int? fromDate = null,
        [Description("End date in YYYYMMDD format (e.g., 20241231)")]
        int? toDate = null,
        [Description("For dimension analysis: R1, R2, R3, etc.")]
        string? dimensionLevel = null,
        [Description("Maximum number of results to return (default: 50, max: 200)")]
        int limit = 50)
    {
        try
        {
            if (limit <= 0 || limit > 200) limit = 50;
            analysisType = analysisType?.ToLower() ?? "customer";

            string sql;
            switch (analysisType)
            {
                case "customer":
                    sql = @"
                        WITH CustomerProfitability AS (
                            SELECT 
                                ol.LiableActor AS CustomerId,
                                a.Nm + ' ' + COALESCE(a.Ad1, '') AS CustomerName,
                                COUNT(DISTINCT ol.OrderNo) AS OrderCount,
                                COUNT(*) AS LineItemCount,
                                SUM(ol.Qty) AS TotalQuantity,
                                SUM(ol.Qty * ol.Price) AS TotalRevenue,
                                SUM(ol.Qty * COALESCE(p.CostPrice, ol.Price * 0.6)) AS EstimatedCost,
                                SUM(ol.Qty * ol.Price) - SUM(ol.Qty * COALESCE(p.CostPrice, ol.Price * 0.6)) AS GrossProfit
                            FROM ordln ol
                            LEFT JOIN actor a ON ol.LiableActor = a.ActorId
                            LEFT JOIN prod p ON ol.ProdNo = p.ProdNo
                            WHERE (@fromDate IS NULL OR ol.OrderDate >= CONVERT(date, CAST(@fromDate AS varchar(8)), 112))
                                AND (@toDate IS NULL OR ol.OrderDate <= CONVERT(date, CAST(@toDate AS varchar(8)), 112))
                            GROUP BY ol.LiableActor, a.Nm, a.Ad1
                        )
                        SELECT TOP (@limit)
                            CustomerId,
                            CustomerName,
                            OrderCount,
                            LineItemCount,
                            TotalQuantity,
                            TotalRevenue,
                            EstimatedCost,
                            GrossProfit,
                            CASE WHEN TotalRevenue > 0 THEN (GrossProfit / TotalRevenue) * 100 ELSE 0 END AS GrossProfitMargin
                        FROM CustomerProfitability
                        WHERE TotalRevenue > 0
                        ORDER BY GrossProfit DESC";
                    break;

                case "product":
                    sql = @"
                        WITH ProductProfitability AS (
                            SELECT 
                                ol.ProdNo AS ProductNumber,
                                p.Desc AS ProductDescription,
                                COUNT(DISTINCT ol.OrderNo) AS OrderCount,
                                COUNT(*) AS LineItemCount,
                                SUM(ol.Qty) AS TotalQuantitySold,
                                SUM(ol.Qty * ol.Price) AS TotalRevenue,
                                SUM(ol.Qty * COALESCE(p.CostPrice, ol.Price * 0.6)) AS EstimatedCost,
                                SUM(ol.Qty * ol.Price) - SUM(ol.Qty * COALESCE(p.CostPrice, ol.Price * 0.6)) AS GrossProfit,
                                AVG(ol.Price) AS AverageSellingPrice
                            FROM ordln ol
                            LEFT JOIN prod p ON ol.ProdNo = p.ProdNo
                            WHERE (@fromDate IS NULL OR ol.OrderDate >= CONVERT(date, CAST(@fromDate AS varchar(8)), 112))
                                AND (@toDate IS NULL OR ol.OrderDate <= CONVERT(date, CAST(@toDate AS varchar(8)), 112))
                            GROUP BY ol.ProdNo, p.Desc, p.CostPrice
                        )
                        SELECT TOP (@limit)
                            ProductNumber,
                            ProductDescription,
                            OrderCount,
                            LineItemCount,
                            TotalQuantitySold,
                            TotalRevenue,
                            EstimatedCost,
                            GrossProfit,
                            CASE WHEN TotalRevenue > 0 THEN (GrossProfit / TotalRevenue) * 100 ELSE 0 END AS GrossProfitMargin,
                            AverageSellingPrice
                        FROM ProductProfitability
                        WHERE TotalRevenue > 0
                        ORDER BY GrossProfit DESC";
                    break;

                default:
                    throw new ArgumentException("Analysis type must be 'customer' or 'product'");
            }

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var fromDateParam = command.CreateParameter();
            fromDateParam.ParameterName = "@fromDate";
            fromDateParam.Value = (object?)fromDate ?? DBNull.Value;
            command.Parameters.Add(fromDateParam);

            var toDateParam = command.CreateParameter();
            toDateParam.ParameterName = "@toDate";
            toDateParam.Value = (object?)toDate ?? DBNull.Value;
            command.Parameters.Add(toDateParam);

            var limitParam = command.CreateParameter();
            limitParam.ParameterName = "@limit";
            limitParam.Value = limit;
            command.Parameters.Add(limitParam);

            var results = new List<object>();
            decimal totalRevenue = 0, totalCost = 0, totalProfit = 0;
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var revenue = reader.GetDecimal(5);
                var cost = reader.GetDecimal(6);
                var profit = reader.GetDecimal(7);
                var margin = reader.GetDecimal(8);

                totalRevenue += revenue;
                totalCost += cost;
                totalProfit += profit;

                if (analysisType == "customer")
                {
                    results.Add(new
                    {
                        CustomerId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        CustomerName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        OrderCount = reader.GetInt32(2),
                        LineItemCount = reader.GetInt32(3),
                        TotalQuantity = reader.GetDecimal(4),
                        TotalRevenue = revenue,
                        EstimatedCost = cost,
                        GrossProfit = profit,
                        GrossProfitMargin = Math.Round(margin, 2)
                    });
                }
                else
                {
                    results.Add(new
                    {
                        ProductNumber = reader.IsDBNull(0) ? "" : reader.GetString(0),
                        ProductDescription = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        OrderCount = reader.GetInt32(2),
                        LineItemCount = reader.GetInt32(3),
                        TotalQuantitySold = reader.GetDecimal(4),
                        TotalRevenue = revenue,
                        EstimatedCost = cost,
                        GrossProfit = profit,
                        GrossProfitMargin = Math.Round(margin, 2),
                        AverageSellingPrice = reader.GetDecimal(9)
                    });
                }
            }

            return JsonSerializer.Serialize(new
            {
                profitabilityAnalysis = new
                {
                    analysisType = analysisType,
                    period = new { fromDate = fromDate, toDate = toDate },
                    summary = new
                    {
                        totalItems = results.Count,
                        totalRevenue = totalRevenue,
                        totalEstimatedCost = totalCost,
                        totalGrossProfit = totalProfit,
                        overallGrossProfitMargin = totalRevenue > 0 ? Math.Round(totalProfit / totalRevenue * 100, 2) : 0
                    },
                    results = results
                },
                insights = new
                {
                    topPerformers = results.Take(5),
                    profitableItemsCount = results.Count(r => ((dynamic)r).GrossProfit > 0),
                    highMarginItemsCount = results.Count(r => ((dynamic)r).GrossProfitMargin > 30)
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }

    // ==================== BUDGET VARIANCE ANALYSIS ====================

    /// <summary>
    /// Compares actual performance against budget by account or dimension
    /// </summary>
    /// <param name="budgetYear">Budget year for comparison (e.g., 2024)</param>
    /// <param name="actualFromDate">Start date for actual data in YYYYMMDD format</param>
    /// <param name="actualToDate">End date for actual data in YYYYMMDD format</param>
    /// <param name="varianceThreshold">Minimum variance percentage to include (default: 5)</param>
    /// <param name="limit">Maximum number of accounts to return (default: 100, max: 200)</param>
    /// <returns>JSON object with budget variance analysis</returns>
    [McpServerTool]
    [Description("Compares actual financial performance against budget showing variances and explanations.")]
    public async Task<string> GetBudgetVarianceAnalysis(
        [Description("Budget year for comparison (e.g., 2024)")]
        int budgetYear,
        [Description("Start date for actual data in YYYYMMDD format")]
        int? actualFromDate = null,
        [Description("End date for actual data in YYYYMMDD format")]
        int? actualToDate = null,
        [Description("Minimum variance percentage to include (default: 5)")]
        decimal varianceThreshold = 5,
        [Description("Maximum number of accounts to return (default: 100, max: 200)")]
        int limit = 100)
    {
        try
        {
            if (limit <= 0 || limit > 200) limit = 100;

            // Note: This is a simplified budget variance analysis since we don't have budget tables
            // In a real implementation, you would join with budget tables
            var sql = @"
                WITH ActualData AS (
                    SELECT 
                        a.AcNo AS AccountNumber,
                        a.Nm AS AccountName,
                        a.AcTp AS AccountType,
                        SUM(COALESCE(at.Db, 0) - COALESCE(at.Cr, 0)) AS ActualAmount
                    FROM ac a
                    LEFT JOIN actr at ON a.AcNo = at.AcNo
                    WHERE (@actualFromDate IS NULL OR at.TrDt >= CONVERT(date, CAST(@actualFromDate AS varchar(8)), 112))
                        AND (@actualToDate IS NULL OR at.TrDt <= CONVERT(date, CAST(@actualToDate AS varchar(8)), 112))
                        AND YEAR(COALESCE(at.TrDt, GETDATE())) = @budgetYear
                    GROUP BY a.AcNo, a.Nm, a.AcTp
                ),
                BudgetEstimate AS (
                    SELECT 
                        AccountNumber,
                        AccountName,
                        AccountType,
                        ActualAmount,
                        -- Estimated budget (in real implementation, this would come from budget tables)
                        CASE 
                            WHEN AccountType IN ('Revenue', 'Sales', 'Income') THEN ActualAmount * 1.1 -- Assume 10% growth target
                            WHEN AccountType IN ('Expense', 'Cost') THEN ActualAmount * 0.95 -- Assume 5% cost reduction target
                            ELSE ActualAmount
                        END AS BudgetAmount
                    FROM ActualData
                ),
                VarianceAnalysis AS (
                    SELECT 
                        AccountNumber,
                        AccountName,
                        AccountType,
                        ActualAmount,
                        BudgetAmount,
                        (ActualAmount - BudgetAmount) AS Variance,
                        CASE 
                            WHEN BudgetAmount <> 0 THEN ((ActualAmount - BudgetAmount) / ABS(BudgetAmount)) * 100
                            ELSE 0
                        END AS VariancePercentage
                    FROM BudgetEstimate
                )
                SELECT TOP (@limit)
                    AccountNumber,
                    AccountName,
                    AccountType,
                    ActualAmount,
                    BudgetAmount,
                    Variance,
                    VariancePercentage
                FROM VarianceAnalysis
                WHERE ABS(VariancePercentage) >= @varianceThreshold
                    AND (ABS(ActualAmount) > 0.01 OR ABS(BudgetAmount) > 0.01)
                ORDER BY ABS(Variance) DESC";

            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            var budgetYearParam = command.CreateParameter();
            budgetYearParam.ParameterName = "@budgetYear";
            budgetYearParam.Value = budgetYear;
            command.Parameters.Add(budgetYearParam);

            var actualFromDateParam = command.CreateParameter();
            actualFromDateParam.ParameterName = "@actualFromDate";
            actualFromDateParam.Value = (object?)actualFromDate ?? DBNull.Value;
            command.Parameters.Add(actualFromDateParam);

            var actualToDateParam = command.CreateParameter();
            actualToDateParam.ParameterName = "@actualToDate";
            actualToDateParam.Value = (object?)actualToDate ?? DBNull.Value;
            command.Parameters.Add(actualToDateParam);

            var limitParam = command.CreateParameter();
            limitParam.ParameterName = "@limit";
            limitParam.Value = limit;
            command.Parameters.Add(limitParam);

            var varianceThresholdParam = command.CreateParameter();
            varianceThresholdParam.ParameterName = "@varianceThreshold";
            varianceThresholdParam.Value = varianceThreshold;
            command.Parameters.Add(varianceThresholdParam);

            var results = new List<object>();
            decimal totalActual = 0, totalBudget = 0, totalVariance = 0;
            
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var actual = reader.GetDecimal(3);
                var budget = reader.GetDecimal(4);
                var variance = reader.GetDecimal(5);
                var variancePercent = reader.GetDecimal(6);

                totalActual += actual;
                totalBudget += budget;
                totalVariance += variance;

                results.Add(new
                {
                    AccountNumber = reader.GetString(0),
                    AccountName = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    AccountType = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ActualAmount = actual,
                    BudgetAmount = budget,
                    Variance = variance,
                    VariancePercentage = Math.Round(variancePercent, 1),
                    VarianceStatus = variance > 0 ? 
                        (reader.GetString(2)?.Contains("Revenue") == true ? "Favorable" : "Unfavorable") :
                        (reader.GetString(2)?.Contains("Revenue") == true ? "Unfavorable" : "Favorable")
                });
            }

            return JsonSerializer.Serialize(new
            {
                budgetVarianceAnalysis = new
                {
                    budgetYear = budgetYear,
                    actualPeriod = new { fromDate = actualFromDate, toDate = actualToDate },
                    summary = new
                    {
                        totalAccountsAnalyzed = results.Count,
                        totalActualAmount = totalActual,
                        totalBudgetAmount = totalBudget,
                        totalVariance = totalVariance,
                        overallVariancePercentage = totalBudget != 0 ? Math.Round(totalVariance / Math.Abs(totalBudget) * 100, 1) : 0
                    },
                    variances = results
                },
                analysis = new
                {
                    favorableVariances = results.Count(r => ((dynamic)r).VarianceStatus == "Favorable"),
                    unfavorableVariances = results.Count(r => ((dynamic)r).VarianceStatus == "Unfavorable"),
                    significantVariances = results.Count(r => Math.Abs(((dynamic)r).VariancePercentage) > 20),
                    note = "Budget amounts are estimated based on targets. In production, actual budget data should be used."
                }
            }, GetJsonOptions());
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new 
            { 
                error = "Database error occurred", 
                message = ex.Message 
            });
        }
    }
}
