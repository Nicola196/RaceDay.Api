using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Api.Models;

public class User
{
    [Key]
    public int UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [ForeignKey(nameof(Role))]
    public int RoleId { get; set; }
    public Role? Role { get; set; }

    public ICollection<EventEnrolment> EventEnrolments { get; set; } = new List<EventEnrolment>();
}