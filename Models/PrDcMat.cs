using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents a price and discount matrix entry from the database
/// </summary>
[Table("PrDcMat")]
public class PrDcMat
{
    [Key]
    [Column("LnNo")]
    public int LineNumber { get; set; }

    [Column("OrdNo")]
    public int? OrderNumber { get; set; }

    [Column("OrdPrGr")]
    public int? OrderPriceGroup { get; set; }

    [MaxLength(255)]
    [Column("ProdNo")]
    public string? ProductNumber { get; set; }

    [Column("ProdPrGr")]
    public int? ProductPriceGroup { get; set; }

    [Column("CustNo")]
    public int? CustomerNumber { get; set; }

    [Column("CustPrGr")]
    public int? CustomerPriceGroup { get; set; }

    [Column("EmpNo")]
    public int? EmployeeNumber { get; set; }

    [Column("EmpPrGr")]
    public int? EmployeePriceGroup { get; set; }

    [Column("SalePr")]
    public decimal? SalePrice { get; set; }

    [Column("SaleDcP")]
    public decimal? SaleDiscountPercent { get; set; }

    [Column("SaleDcAm")]
    public decimal? SaleDiscountAmount { get; set; }

    [Column("CompPr")]
    public decimal? CompanyPrice { get; set; }

    [Column("SugPr")]
    public decimal? SuggestedPrice { get; set; }

    [Column("CstPr")]
    public decimal? CostPrice { get; set; }

    [Column("PurcPr")]
    public decimal? PurchasePrice { get; set; }

    [Column("PurcDcP")]
    public decimal? PurchaseDiscountPercent { get; set; }

    [Column("PurcDcAm")]
    public decimal? PurchaseDiscountAmount { get; set; }

    // Navigation properties
    public virtual Prod? Product { get; set; }
}
