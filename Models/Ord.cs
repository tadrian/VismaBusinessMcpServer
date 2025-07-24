using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents an order document from the Ord table
/// </summary>
[Table("Ord")]
public class Ord
{
    /// <summary>
    /// Order number - primary key
    /// </summary>
    [Key]
    [Column("OrdNo")]
    public int OrderNumber { get; set; }

    /// <summary>
    /// Order type
    /// </summary>
    [Column("OrdTp")]
    public byte OrderType { get; set; }

    /// <summary>
    /// Order date in YYYYMMDD format
    /// </summary>
    [Column("OrdDt")]
    public int OrderDate { get; set; }

    /// <summary>
    /// Customer number
    /// </summary>
    [Column("CustNo")]
    public int CustomerNumber { get; set; }

    /// <summary>
    /// Customer name
    /// </summary>
    [Column("Nm")]
    public string? CustomerName { get; set; }

    /// <summary>
    /// Order status
    /// </summary>
    [Column("EdSt")]
    public int Status { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    [Column("Cur")]
    public int Currency { get; set; }

    /// <summary>
    /// Our reference
    /// </summary>
    [Column("OurRef")]
    public string? Reference { get; set; }

    /// <summary>
    /// Total order amount
    /// </summary>
    [Column("OrdSum")]
    public decimal Amount { get; set; }

    /// <summary>
    /// VAT amount
    /// </summary>
    [Column("VatAm")]
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Delivery actor number
    /// </summary>
    [Column("DelActNo")]
    public int? DeliveryActorNumber { get; set; }

    /// <summary>
    /// Liable actor number (customer)
    /// </summary>
    [Column("LiaActNo")]
    public int? LiableActorNumber { get; set; }

    /// <summary>
    /// Ship actor number
    /// </summary>
    [Column("ShpActNo")]
    public int? ShipActorNumber { get; set; }

    /// <summary>
    /// Factory number
    /// </summary>
    [Column("FactNo")]
    [MaxLength(255)]
    public string? FactoryNumber { get; set; }

    /// <summary>
    /// Delivery date in YYYYMMDD format
    /// </summary>
    [Column("DelDt")]
    public int? DeliveryDate { get; set; }

    /// <summary>
    /// Navigation property to order lines
    /// </summary>
    public virtual ICollection<OrdLn> OrderLines { get; set; } = new List<OrdLn>();

    // Navigation properties temporarily disabled due to mapping issues
    /*
    /// <summary>
    /// Navigation property to delivery actor
    /// </summary>
    public virtual Actor? DeliveryActor { get; set; }

    /// <summary>
    /// Navigation property to liable actor (customer)
    /// </summary>
    public virtual Actor? LiableActor { get; set; }

    /// <summary>
    /// Navigation property to ship actor
    /// </summary>
    public virtual Actor? ShipActor { get; set; }
    */
}
