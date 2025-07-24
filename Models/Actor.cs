using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents an actor (business entity) from the database
/// </summary>
[Table("Actor")]
public class Actor
{
    [Key]
    [Column("ActNo")]
    public int ActorId { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("Nm")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Ad1")]
    public string Address1 { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("Ad2")]
    public string Address2 { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("PNo")]
    public string PostalCode { get; set; } = string.Empty;

    [MaxLength(100)]
    [Column("PArea")]
    public string PostalArea { get; set; } = string.Empty;

    [MaxLength(255)]
    [Column("MailAd")]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    [Column("Phone")]
    public string Phone { get; set; } = string.Empty;

    [Column("CustNo")]
    public int CustomerNumber { get; set; }

    [Column("SupNo")]
    public int SupplierNumber { get; set; }

    [Column("ChDt")]
    public int LastUpdate { get; set; }
}
