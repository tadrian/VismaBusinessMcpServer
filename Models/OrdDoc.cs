using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlMcpServer.Models;

/// <summary>
/// Represents an order document (invoice, order, etc.) from the database
/// </summary>
[Table("OrdDoc")]
public class OrdDoc
{
    [Key]
    [Column("OrdDocNo")]
    public int OrderDocumentNumber { get; set; }

    [Column("DocTp")]
    public int DocumentType { get; set; }

    [Column("DocDt")]
    public int DocumentDate { get; set; }

    [Column("DelActNo")]
    public int? DeliveryActorNumber { get; set; }

    [Column("LiaActNo")]
    public int? LiableActorNumber { get; set; }

    [Column("ShpActNo")]
    public int? ShipActorNumber { get; set; }

    [MaxLength(255)]
    [Column("FactNo")]
    public string? FactoryNumber { get; set; }

    // Navigation properties
    public virtual ICollection<OrdDocLn> OrderLines { get; set; } = new List<OrdDocLn>();

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
}
