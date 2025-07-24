using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents a product from the database
/// </summary>
[Table("Prod")]
public class Prod
{
    [Key]
    [MaxLength(255)]
    [Column("ProdNo")]
    public string ProductNumber { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Descr")]
    public string Description { get; set; } = string.Empty;

    [Column("ProdGr")]
    public int? ProductGroup { get; set; }

    [Column("ProdTp")]
    public int? ProductType { get; set; }

    [Column("StSaleUn")]
    public int? StandardSalesUnit { get; set; }

    [Column("ProdPrGr")]
    public int? ProductPriceGroup { get; set; }

    // Navigation properties
    public virtual ICollection<OrdDocLn> OrderLines { get; set; } = new List<OrdDocLn>();
}
