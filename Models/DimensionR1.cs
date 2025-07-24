using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents dimension table R1 (integer key)
/// </summary>
[Table("R1")]
public class DimensionR1
{
    /// <summary>
    /// Dimension key (integer)
    /// </summary>
    [Key]
    [Column("RNo")]
    public int DimensionKey { get; set; }

    /// <summary>
    /// Dimension name or description
    /// </summary>
    [Column("Nm")]
    public string? Name { get; set; }

    /// <summary>
    /// Address line 1
    /// </summary>
    [Column("Ad1")]
    public string? Address1 { get; set; }

    /// <summary>
    /// Status (active flag)
    /// </summary>
    [Column("St")]
    public byte Status { get; set; }

    /// <summary>
    /// Suspended flag
    /// </summary>
    [Column("Susp")]
    public byte Suspended { get; set; }

    /// <summary>
    /// Main R1 (parent key)
    /// </summary>
    [Column("MainR1")]
    public int? ParentKey { get; set; }

    /// <summary>
    /// Created date in YYYYMMDD format
    /// </summary>
    [Column("CreDt")]
    public int? CreatedDate { get; set; }
}
