using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents an order document line item from the database
/// </summary>
[Table("OrdDocLn")]
public class OrdDocLn
{
    [Key]
    [Column("OrdDocNo")]
    public int OrderDocumentNumber { get; set; }

    [Key]
    [Column("DocLnNo")]
    public int DocumentLineNumber { get; set; }

    [Column("TrDt")]
    public int TransactionDate { get; set; }

    [MaxLength(255)]
    [Column("ProdNo")]
    public string ProductNumber { get; set; } = string.Empty;

    [Column("ExQty")]
    public decimal? ExecutedQuantity { get; set; }

    [Column("Price")]
    public decimal? Price { get; set; }

    [Column("DPrice")]
    public decimal? DiscountedPrice { get; set; }

    [Column("Am")]
    public decimal? Amount { get; set; }

    [Column("DAm")]
    public decimal? DiscountedAmount { get; set; }

    [Column("VatAm")]
    public decimal? VatAmount { get; set; }

    [Column("DVatAm")]
    public decimal? DiscountedVatAmount { get; set; }

    // Navigation properties
    public virtual OrdDoc? OrderDocument { get; set; }
    public virtual Prod? Product { get; set; }
}
