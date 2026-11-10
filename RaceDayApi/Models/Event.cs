namespace RaceDayApi.Models;

public class Event
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }

    // Run, Walk or Cycle
    public string EventType { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? MapUrl { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public ICollection<EventEnrolment> Enrolments { get; set; } = new List<EventEnrolment>();
}