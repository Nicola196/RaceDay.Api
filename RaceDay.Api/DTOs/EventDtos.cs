using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class EventDto
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string? EventDescription { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int EnrolmentCount { get; set; }
}

public class CreateEventDto
{
    [Required, MaxLength(120)]
    public string EventName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? EventDescription { get; set; }

    [Required]
    public DateTime EventDate { get; set; }

    [Required, MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    [MaxLength(20)]
    public string? Distance { get; set; }

    [Required]
    [RegularExpression("Run|Walk|Cycle", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
    public string EventType { get; set; } = "Run";

    [Required]
    public int CategoryId { get; set; }
}

public class UpdateEventDto : CreateEventDto
{
}