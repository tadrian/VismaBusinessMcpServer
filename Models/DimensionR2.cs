using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents dimension table R2 (integer key)
/// </summary>
[Table("R2")]
public class DimensionR2
{
    /// <summary>
    /// Dimension key (integer)
    /// </summary>
    [Key]
    [Column("R2")]
    public int DimensionKey { get; set; }

    /// <summary>
    /// Dimension name or description
    /// </summary>
    [Column("Nm")]
    public string? Name { get; set; }

    /// <summary>
    /// Additional description
    /// </summary>
    [Column("Desc")]
    public string? Description { get; set; }

    /// <summary>
    /// Whether the dimension is active
    /// </summary>
    [Column("Act")]
    public bool? Active { get; set; }

    /// <summary>
    /// Parent dimension key for hierarchical structure
    /// </summary>
    [Column("ParR2")]
    public int? ParentKey { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    [Column("CrDt")]
    public DateTime? CreatedDate { get; set; }
}
