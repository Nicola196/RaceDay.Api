using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Models;

public class EventEnrolment
{
    [Key]
    public int EnrolmentId { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public DateTime EnrolmentDate { get; set; }

    // Optional: a result only exists after the race
    public RaceResult? Result { get; set; }
}