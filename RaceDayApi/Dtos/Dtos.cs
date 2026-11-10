using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using RaceDayApi.Models;

namespace RaceDayApi.Dtos;

// ---------- Authentication & profile ----------

public class RegisterRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    /// <summary>1 = Organiser, 2 = Member</summary>
    [Required]
    public int RoleId { get; set; }
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = string.Empty;
}

public class UserResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// ---------- Categories ----------

public class CategoryRequest
{
    [Required, StringLength(100)]
    public string CategoryName { get; set; } = string.Empty;
}

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

// ---------- Events ----------

public class EventRequest
{
    [Required, StringLength(150)]
    public string EventName { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string EventDescription { get; set; } = string.Empty;

    [Required]
    public DateTime EventDate { get; set; }

    [Required, StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public decimal DistanceKm { get; set; }

    /// <summary>Run, Walk or Cycle</summary>
    [Required, RegularExpression("^(Run|Walk|Cycle)$", ErrorMessage = "EventType must be Run, Walk or Cycle.")]
    public string EventType { get; set; } = string.Empty;

    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Range(-180, 180)]
    public double Longitude { get; set; }

    public string? MapUrl { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}

public class EventResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string EventDescription { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string EventType { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? MapUrl { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public static readonly Expression<Func<Event, EventResponse>> Projection = e => new EventResponse
    {
        EventId = e.EventId,
        EventName = e.EventName,
        EventDescription = e.EventDescription,
        EventDate = e.EventDate,
        Location = e.Location,
        DistanceKm = e.DistanceKm,
        EventType = e.EventType,
        Latitude = e.Latitude,
        Longitude = e.Longitude,
        MapUrl = e.MapUrl,
        CategoryId = e.CategoryId,
        CategoryName = e.Category.CategoryName
    };
}

public class LocationResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? MapUrl { get; set; }
}

public class WeatherResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Conditions { get; set; } = string.Empty;
    public double? MaxTempC { get; set; }
    public double? MinTempC { get; set; }
    public double? RainChancePercent { get; set; }
    public double? MaxWindKmh { get; set; }
}

// ---------- Enrolments ----------

public class EnrolmentResponse
{
    public int EnrolmentId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime EnrolmentDate { get; set; }

    public static readonly Expression<Func<EventEnrolment, EnrolmentResponse>> Projection = x => new EnrolmentResponse
    {
        EnrolmentId = x.EnrolmentId,
        UserId = x.UserId,
        FullName = x.User.FullName,
        Email = x.User.Email,
        EventId = x.EventId,
        EventName = x.Event.EventName,
        EventDate = x.Event.EventDate,
        EnrolmentDate = x.EnrolmentDate
    };
}

// ---------- Results ----------

public class ResultRequest
{
    /// <summary>Format hh:mm:ss, e.g. 01:45:30</summary>
    [Required]
    public TimeSpan FinishTime { get; set; }

    [Range(1, int.MaxValue)]
    public int Position { get; set; }
}

public class ResultResponse
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int Position { get; set; }

    public static readonly Expression<Func<RaceResult, ResultResponse>> Projection = r => new ResultResponse
    {
        ResultId = r.ResultId,
        EnrolmentId = r.EnrolmentId,
        EventId = r.Enrolment.EventId,
        EventName = r.Enrolment.Event.EventName,
        FullName = r.Enrolment.User.FullName,
        FinishTime = r.FinishTime,
        Position = r.Position
    };
}