using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.Categories
            .OrderBy(c => c.CategoryId)
            .Select(c => new CategoryDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var name = dto.CategoryName.Trim();

        if (await _db.Categories.AnyAsync(c => c.CategoryName == name))
            return Conflict(new { message = "A category with that name already exists." });

        var category = new Category { CategoryName = name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { id = category.CategoryId }, new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName
        });
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound(new { message = $"Category {id} does not exist." });

        category.CategoryName = dto.CategoryName.Trim();
        await _db.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName
        });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Organiser")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
            return NotFound(new { message = $"Category {id} does not exist." });

        var inUse = await _db.Events.AnyAsync(e => e.CategoryId == id);
        if (inUse)
            return Conflict(new { message = "Cannot delete a category that still has events linked to it." });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}