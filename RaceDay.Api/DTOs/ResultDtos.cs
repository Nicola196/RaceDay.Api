using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CreateResultDto
{
    [Required]
    public TimeSpan FinishTime { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Position must be 1 or greater.")]
    public int Position { get; set; }
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }
}