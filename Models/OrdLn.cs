using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents an order line from the OrdLn table
/// </summary>
[Table("OrdLn")]
public class OrdLn
{
    /// <summary>
    /// Order number - part of composite key
    /// </summary>
    [Column("OrdNo")]
    public int OrderNumber { get; set; }

    /// <summary>
    /// Line number - part of composite key
    /// </summary>
    [Column("LnNo")]
    public int LineNumber { get; set; }

    /// <summary>
    /// Product number
    /// </summary>
    [Column("ProdNo")]
    [MaxLength(50)]
    public string? ProductNumber { get; set; }

    /// <summary>
    /// Transaction date
    /// </summary>
    [Column("TrDt")]
    public int TransactionDate { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    [Column("Descr")]
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Quantity registered
    /// </summary>
    [Column("NoReg")]
    public decimal NoRegistered { get; set; }

    /// <summary>
    /// Quantity invoiced
    /// </summary>
    [Column("NoInvoAb")]
    public decimal NoInvoiced { get; set; }

    /// <summary>
    /// Unit price
    /// </summary>
    [Column("Price")]
    public decimal Price { get; set; }

    /// <summary>
    /// Domestic unit price
    /// </summary>
    [Column("DPrice")]
    public decimal DomesticPrice { get; set; }

    /// <summary>
    /// Line amount
    /// </summary>
    [Column("Am")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Domestic line amount
    /// </summary>
    [Column("DAm")]
    public decimal DomesticAmount { get; set; }

    /// <summary>
    /// VAT amount
    /// </summary>
    [Column("VatAm")]
    public decimal VatAmount { get; set; }

    /// <summary>
    /// Domestic VAT amount
    /// </summary>
    [Column("DVatAm")]
    public decimal DomesticVatAmount { get; set; }

    /// <summary>
    /// Delivery date for this line
    /// </summary>
    [Column("DelDt")]
    public int DeliveryDate { get; set; }

    /// <summary>
    /// Financial dimension R1
    /// </summary>
    [Column("R1")]
    public int? R1 { get; set; }

    /// <summary>
    /// Financial dimension R2
    /// </summary>
    [Column("R2")]
    public int? R2 { get; set; }

    /// <summary>
    /// Financial dimension R3
    /// </summary>
    [Column("R3")]
    public int? R3 { get; set; }

    /// <summary>
    /// Financial dimension R4
    /// </summary>
    [Column("R4")]
    public int? R4 { get; set; }

    /// <summary>
    /// Financial dimension R5
    /// </summary>
    [Column("R5")]
    public int? R5 { get; set; }

    /// <summary>
    /// Financial dimension R6
    /// </summary>
    [Column("R6")]
    public int? R6 { get; set; }

    /// <summary>
    /// Financial dimension R7
    /// </summary>
    [Column("R7")]
    [MaxLength(50)]
    public string? R7 { get; set; }

    /// <summary>
    /// Financial dimension R8
    /// </summary>
    [Column("R8")]
    [MaxLength(50)]
    public string? R8 { get; set; }

    /// <summary>
    /// Financial dimension R9
    /// </summary>
    [Column("R9")]
    [MaxLength(50)]
    public string? R9 { get; set; }

    /// <summary>
    /// Financial dimension R10
    /// </summary>
    [Column("R10")]
    [MaxLength(50)]
    public string? R10 { get; set; }

    /// <summary>
    /// Financial dimension R11
    /// </summary>
    [Column("R11")]
    [MaxLength(50)]
    public string? R11 { get; set; }

    /// <summary>
    /// Financial dimension R12
    /// </summary>
    [Column("R12")]
    [MaxLength(50)]
    public string? R12 { get; set; }

    /// <summary>
    /// Navigation property to Order
    /// </summary>
    public virtual Ord? Order { get; set; }

    /// <summary>
    /// Navigation property to Product
    /// </summary>
    public virtual Prod? Product { get; set; }

    // Financial dimension navigation properties temporarily disabled due to mapping issues
    /*
    /// <summary>
    /// Navigation property to R1 dimension
    /// </summary>
    public virtual DimensionR1? DimensionR1 { get; set; }

    /// <summary>
    /// Navigation property to R2 dimension
    /// </summary>
    public virtual DimensionR2? DimensionR2 { get; set; }

    /// <summary>
    /// Navigation property to R3 dimension
    /// </summary>
    public virtual DimensionR3? DimensionR3 { get; set; }

    /// <summary>
    /// Navigation property to R4 dimension
    /// </summary>
    public virtual DimensionR4? DimensionR4 { get; set; }

    /// <summary>
    /// Navigation property to R5 dimension
    /// </summary>
    public virtual DimensionR5? DimensionR5 { get; set; }

    /// <summary>
    /// Navigation property to R6 dimension
    /// </summary>
    public virtual DimensionR6? DimensionR6 { get; set; }

    /// <summary>
    /// Navigation property to R7 dimension
    /// </summary>
    public virtual DimensionR7? DimensionR7 { get; set; }

    /// <summary>
    /// Navigation property to R8 dimension
    /// </summary>
    public virtual DimensionR8? DimensionR8 { get; set; }

    /// <summary>
    /// Navigation property to R9 dimension
    /// </summary>
    public virtual DimensionR9? DimensionR9 { get; set; }

    /// <summary>
    /// Navigation property to R10 dimension
    /// </summary>
    public virtual DimensionR10? DimensionR10 { get; set; }

    /// <summary>
    /// Navigation property to R11 dimension
    /// </summary>
    public virtual DimensionR11? DimensionR11 { get; set; }

    /// <summary>
    /// Navigation property to R12 dimension
    /// </summary>
    public virtual DimensionR12? DimensionR12 { get; set; }
    */
}
