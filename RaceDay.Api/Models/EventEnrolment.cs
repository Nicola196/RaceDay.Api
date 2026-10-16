using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class EventEnrolment
{
    [Key]
    public int EnrolmentId { get; set; }

    [ForeignKey(nameof(User))]
    public int UserId { get; set; }
    public User? User { get; set; }

    [ForeignKey(nameof(Event))]
    public int EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;

    public Result? Result { get; set; }
}