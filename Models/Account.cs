using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents an account in the accounting system (ac table)
/// </summary>
[Table("Ac")]
public class Account
{
    /// <summary>
    /// Account number - primary key
    /// </summary>
    [Key]
    [Column("AcNo")]
    public int AccountNumber { get; set; }

    /// <summary>
    /// Account name or description
    /// </summary>
    [Column("Nm")]
    public string? Name { get; set; }

    /// <summary>
    /// Account code
    /// </summary>
    [Column("AcCode")]
    public string? AccountCode { get; set; }

    /// <summary>
    /// Account group or classification
    /// </summary>
    [Column("AcGr")]
    public string? AccountGroup { get; set; }

    /// <summary>
    /// Responsible (Res column)
    /// </summary>
    [Column("Res")]
    public byte? Responsible { get; set; }

    /// <summary>
    /// Tax class
    /// </summary>
    [Column("TxCl")]
    public byte? TaxClass { get; set; }

    /// <summary>
    /// Tax code
    /// </summary>
    [Column("TxCd")]
    public int? TaxCode { get; set; }

    /// <summary>
    /// Transient flag
    /// </summary>
    [Column("Trn")]
    public byte? Transient { get; set; }

    /// <summary>
    /// Suspended flag
    /// </summary>
    [Column("Susp")]
    public byte? Suspended { get; set; }

    /// <summary>
    /// Change date
    /// </summary>
    [Column("ChDt")]
    public int? ChangeDate { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    [Column("CreDt")]
    public int? CreatedDate { get; set; }
}
