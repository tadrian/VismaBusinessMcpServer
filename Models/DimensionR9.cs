using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents dimension table R9 (string key)
/// </summary>
[Table("R9")]
public class DimensionR9
{
    /// <summary>
    /// Dimension key (string)
    /// </summary>
    [Key]
    [Column("R9")]
    public string DimensionKey { get; set; } = string.Empty;

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
    [Column("ParR9")]
    public string? ParentKey { get; set; }

    /// <summary>
    /// Created date
    /// </summary>
    [Column("CrDt")]
    public DateTime? CreatedDate { get; set; }
}
