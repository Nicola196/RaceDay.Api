using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class Result
{
    [Key]
    public int ResultId { get; set; }

    [ForeignKey(nameof(EventEnrolment))]
    public int EnrolmentId { get; set; }
    public EventEnrolment? EventEnrolment { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int Position { get; set; }
}