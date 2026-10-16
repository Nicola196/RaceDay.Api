using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Models;

public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}