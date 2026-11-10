using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;
using RaceDayApi.Security;

namespace RaceDayApi.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    /// <summary>Lists all event categories (public).</summary>
    /// <response code="200">Array of categories.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryResponse>), 200)]
    public async Task<ActionResult<List<CategoryResponse>>> GetCategories()
    {
        return await _db.Categories
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryResponse { CategoryId = c.CategoryId, CategoryName = c.CategoryName })
            .ToListAsync();
    }

    /// <summary>Creates a new category (Organiser only).</summary>
    /// <remarks>Expects: categoryName, e.g. "10km" or "Under 20".</remarks>
    /// <response code="201">Category created.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    [HttpPost]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreateCategory(CategoryRequest request)
    {
        var category = new Category { CategoryName = request.CategoryName.Trim() };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return StatusCode(201, new CategoryResponse
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName
        });
    }

    /// <summary>Renames an existing category (Organiser only).</summary>
    /// <remarks>Expects: categoryName.</remarks>
    /// <response code="200">Returns the updated category.</response>
    /// <response code="400">Invalid data.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    /// <response code="404">Category does not exist.</response>
    [HttpPut("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(typeof(CategoryResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory(int id, CategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        category.CategoryName = request.CategoryName.Trim();
        await _db.SaveChangesAsync();

        return Ok(new CategoryResponse { CategoryId = category.CategoryId, CategoryName = category.CategoryName });
    }

    /// <summary>Deletes a category (Organiser only).</summary>
    /// <response code="204">Deleted.</response>
    /// <response code="401">Not logged in.</response>
    /// <response code="403">Not an Organiser.</response>
    /// <response code="404">Category does not exist.</response>
    /// <response code="409">Category is still used by events and cannot be deleted.</response>
    [HttpDelete("{id:int}")]
    [SessionAuthorize("Organiser")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null)
        {
            return NotFound(new { message = "Category not found." });
        }

        if (await _db.Events.AnyAsync(e => e.CategoryId == id))
        {
            return Conflict(new { message = "This category is used by one or more events." });
        }

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}