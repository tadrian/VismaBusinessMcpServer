using Microsoft.EntityFrameworkCore;
using SqlMcpServer.Models;

namespace SqlMcpServer.Data;

/// <summary>
/// Database context for the SQL MCP server
/// </summary>
public class SqlMcpDbContext : DbContext
{
    public SqlMcpDbContext(DbContextOptions<SqlMcpDbContext> options) : base(options)
    {
    }

    public DbSet<Actor> Actors { get; set; }
    public DbSet<OrdDoc> OrderDocuments { get; set; }
    public DbSet<OrdDocLn> OrderDocumentLines { get; set; }
    public DbSet<Ord> Orders { get; set; }
    public DbSet<OrdLn> OrderLines { get; set; }
    public DbSet<Prod> Products { get; set; }
    public DbSet<PrDcMat> PriceMatrix { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountTransaction> AccountTransactions { get; set; }
    public DbSet<CustomerTransaction> CustomerTransactions { get; set; }
    public DbSet<SupplierTransaction> SupplierTransactions { get; set; }
    
    // Financial Dimension Tables (R1-R12)
    public DbSet<DimensionR1> DimensionsR1 { get; set; }
    public DbSet<DimensionR2> DimensionsR2 { get; set; }
    public DbSet<DimensionR3> DimensionsR3 { get; set; }
    public DbSet<DimensionR4> DimensionsR4 { get; set; }
    public DbSet<DimensionR5> DimensionsR5 { get; set; }
    public DbSet<DimensionR6> DimensionsR6 { get; set; }
    public DbSet<DimensionR7> DimensionsR7 { get; set; }
    public DbSet<DimensionR8> DimensionsR8 { get; set; }
    public DbSet<DimensionR9> DimensionsR9 { get; set; }
    public DbSet<DimensionR10> DimensionsR10 { get; set; }
    public DbSet<DimensionR11> DimensionsR11 { get; set; }
    public DbSet<DimensionR12> DimensionsR12 { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Actor entity
        modelBuilder.Entity<Actor>(entity =>
        {
            entity.HasKey(e => e.ActorId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Address1).HasMaxLength(255);
        });

        // Configure OrdDoc entity
        modelBuilder.Entity<OrdDoc>(entity =>
        {
            entity.HasKey(e => e.OrderDocumentNumber);

            // Remove the problematic navigation property relationships for now
            // The foreign keys are already properly mapped via the [Column] attributes in the model
        });

        // Configure OrdDocLn entity with composite key
        modelBuilder.Entity<OrdDocLn>(entity =>
        {
            entity.HasKey(e => new { e.OrderDocumentNumber, e.DocumentLineNumber });
            
            // Configure relationships
            entity.HasOne(e => e.OrderDocument)
                  .WithMany(od => od.OrderLines)
                  .HasForeignKey(e => e.OrderDocumentNumber)
                  .HasPrincipalKey(od => od.OrderDocumentNumber);

            entity.HasOne(e => e.Product)
                  .WithMany(p => p.OrderLines)
                  .HasForeignKey(e => e.ProductNumber)
                  .HasPrincipalKey(p => p.ProductNumber);
        });

        // Configure Ord entity
        modelBuilder.Entity<Ord>(entity =>
        {
            entity.HasKey(e => e.OrderNumber);
            entity.Property(e => e.FactoryNumber).HasMaxLength(255);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Reference).HasMaxLength(100);

            // Remove the problematic navigation property relationships for now
            // The foreign keys are already properly mapped via the [Column] attributes in the model
        });

        // Configure OrdLn entity with composite key
        modelBuilder.Entity<OrdLn>(entity =>
        {
            entity.HasKey(e => new { e.OrderNumber, e.LineNumber });
            entity.Property(e => e.ProductNumber).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.R7).HasMaxLength(50);
            entity.Property(e => e.R8).HasMaxLength(50);
            entity.Property(e => e.R9).HasMaxLength(50);
            entity.Property(e => e.R10).HasMaxLength(50);
            entity.Property(e => e.R11).HasMaxLength(50);
            entity.Property(e => e.R12).HasMaxLength(50);

            // Configure basic relationships without dimensions for now
            entity.HasOne(e => e.Order)
                  .WithMany(o => o.OrderLines)
                  .HasForeignKey(e => e.OrderNumber)
                  .HasPrincipalKey(o => o.OrderNumber);

            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductNumber)
                  .HasPrincipalKey(p => p.ProductNumber);
        });

        // Configure Prod entity
        modelBuilder.Entity<Prod>(entity =>
        {
            entity.HasKey(e => e.ProductNumber);
            entity.Property(e => e.Description).HasMaxLength(255);
        });

        // Configure PrDcMat entity
        modelBuilder.Entity<PrDcMat>(entity =>
        {
            entity.HasKey(e => e.LineNumber);
            
            // Configure relationships
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductNumber)
                  .HasPrincipalKey(p => p.ProductNumber);
        });

        // Configure Account entity
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountNumber);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.AccountGroup).HasMaxLength(60);  // Match AcGr column size
        });

        // Configure AccountTransaction entity
        modelBuilder.Entity<AccountTransaction>(entity =>
        {
            entity.HasKey(e => new { e.JournalNumber, e.EntryNumber });
            entity.Property(e => e.Description).HasMaxLength(500);

            // Configure relationship to Account
            entity.HasOne(e => e.Account)
                  .WithMany()
                  .HasForeignKey(e => e.AccountNumber)
                  .HasPrincipalKey(a => a.AccountNumber);
        });

        // Configure CustomerTransaction entity
        modelBuilder.Entity<CustomerTransaction>(entity =>
        {
            entity.HasKey(e => new { e.JournalNumber, e.EntryNumber });
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.R7).HasMaxLength(50);
            entity.Property(e => e.R8).HasMaxLength(50);
            entity.Property(e => e.R9).HasMaxLength(50);
            entity.Property(e => e.R10).HasMaxLength(50);
            entity.Property(e => e.R11).HasMaxLength(50);
            entity.Property(e => e.R12).HasMaxLength(50);

            // Configure relationship to Customer (Actor)
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerNumber)
                  .HasPrincipalKey(a => a.ActorId);
        });

        // Configure SupplierTransaction entity
        modelBuilder.Entity<SupplierTransaction>(entity =>
        {
            entity.HasKey(e => new { e.JournalNumber, e.EntryNumber });
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.R7).HasMaxLength(50);
            entity.Property(e => e.R8).HasMaxLength(50);
            entity.Property(e => e.R9).HasMaxLength(50);
            entity.Property(e => e.R10).HasMaxLength(50);
            entity.Property(e => e.R11).HasMaxLength(50);
            entity.Property(e => e.R12).HasMaxLength(50);

            // Configure relationship to Supplier (Actor)
            entity.HasOne(e => e.Supplier)
                  .WithMany()
                  .HasForeignKey(e => e.SupplierNumber)
                  .HasPrincipalKey(a => a.ActorId);
        });

        // Configure Dimension Tables R1-R6 (integer keys)
        modelBuilder.Entity<DimensionR1>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<DimensionR2>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<DimensionR3>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<DimensionR4>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<DimensionR5>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        modelBuilder.Entity<DimensionR6>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // Configure Dimension Tables R7-R12 (string keys)
        modelBuilder.Entity<DimensionR7>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.DimensionKey).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentKey).HasMaxLength(50);
        });

        modelBuilder.Entity<DimensionR8>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.DimensionKey).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentKey).HasMaxLength(50);
        });

        modelBuilder.Entity<DimensionR9>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.DimensionKey).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentKey).HasMaxLength(50);
        });

        modelBuilder.Entity<DimensionR10>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.DimensionKey).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentKey).HasMaxLength(50);
        });

        modelBuilder.Entity<DimensionR11>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.DimensionKey).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentKey).HasMaxLength(50);
        });

        modelBuilder.Entity<DimensionR12>(entity =>
        {
            entity.HasKey(e => e.DimensionKey);
            entity.Property(e => e.DimensionKey).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ParentKey).HasMaxLength(50);
        });
    }
}
