using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents a supplier transaction (SupTr table)
/// </summary>
[Table("SupTr")]
public class SupplierTransaction
{
    /// <summary>
    /// Journal number - part of composite key
    /// </summary>
    [Key]
    [Column("JNo")]
    public int JournalNumber { get; set; }

    /// <summary>
    /// Entry number - part of composite key
    /// </summary>
    [Column("EntNo")]
    public int EntryNumber { get; set; }

    /// <summary>
    /// Supplier number
    /// </summary>
    [Column("SupNo")]
    public int SupplierNumber { get; set; }

    /// <summary>
    /// Voucher number
    /// </summary>
    [Column("VoNo")]
    public int VoucherNumber { get; set; }

    /// <summary>
    /// Voucher date in YYYYMMDD format
    /// </summary>
    [Column("VoDt")]
    public int VoucherDate { get; set; }

    /// <summary>
    /// Due date in YYYYMMDD format
    /// </summary>
    [Column("DueDt")]
    public int DueDate { get; set; }

    /// <summary>
    /// Transaction description
    /// </summary>
    [Column("Txt")]
    [MaxLength(255)]
    public string? Description { get; set; }

    /// <summary>
    /// Invoice number
    /// </summary>
    [Column("InvoNo")]
    [MaxLength(50)]
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Currency amount
    /// </summary>
    [Column("CurAm")]
    public decimal CurrencyAmount { get; set; }

    /// <summary>
    /// Amount in base currency
    /// </summary>
    [Column("Am")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Remaining currency amount
    /// </summary>
    [Column("RCurAm")]
    public decimal RemainingCurrencyAmount { get; set; }

    /// <summary>
    /// Remaining amount in base currency
    /// </summary>
    [Column("RAm")]
    public decimal RemainingAmount { get; set; }

    /// <summary>
    /// Credit flag (0 = debit, 1 = credit)
    /// </summary>
    [Column("Cr")]
    public byte Credit { get; set; }

    /// <summary>
    /// Currency
    /// </summary>
    [Column("Cur")]
    public int Currency { get; set; }

    /// <summary>
    /// Exchange rate
    /// </summary>
    [Column("ExRt")]
    public decimal ExchangeRate { get; set; }

    /// <summary>
    /// Order number
    /// </summary>
    [Column("OrdNo")]
    public int OrderNumber { get; set; }

    /// <summary>
    /// Financial dimension R1
    /// </summary>
    [Column("R1")]
    public int R1 { get; set; }

    /// <summary>
    /// Financial dimension R2
    /// </summary>
    [Column("R2")]
    public int R2 { get; set; }

    /// <summary>
    /// Financial dimension R3
    /// </summary>
    [Column("R3")]
    public int R3 { get; set; }

    /// <summary>
    /// Financial dimension R4
    /// </summary>
    [Column("R4")]
    public int R4 { get; set; }

    /// <summary>
    /// Financial dimension R5
    /// </summary>
    [Column("R5")]
    public int R5 { get; set; }

    /// <summary>
    /// Financial dimension R6
    /// </summary>
    [Column("R6")]
    public int R6 { get; set; }

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
    /// Navigation property to Supplier
    /// </summary>
    public virtual Actor? Supplier { get; set; }
}
