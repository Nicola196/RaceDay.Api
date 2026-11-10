using System.ComponentModel.DataAnnotations;

namespace RaceDayApi.Models;

public class RaceResult
{
    [Key]
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }
    public EventEnrolment Enrolment { get; set; } = null!;

    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}