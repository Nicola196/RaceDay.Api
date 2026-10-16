using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public class CategoryDto
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class CreateCategoryDto
{
    [Required, MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;
}