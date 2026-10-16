using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Event
{
    [Key]
    public int EventId { get; set; }

    [Required]
    [MaxLength(120)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? EventDescription { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [MaxLength(20)]
    public string? Distance { get; set; }

    [Required]
    [MaxLength(20)]
    public string EventType { get; set; } = "Run";

    [ForeignKey(nameof(Category))]
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<EventEnrolment> EventEnrolments { get; set; } = new List<EventEnrolment>();
}