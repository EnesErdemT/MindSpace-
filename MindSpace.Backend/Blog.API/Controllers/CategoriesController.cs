using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IUnitOfWork unitOfWork, ILogger<CategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var categories = await _unitOfWork.Categories.GetAllAsync();
            
            var categoryDtos = categories.Select(c => new
            {
                c.Id,
                c.Name,
                c.Description,
                c.Slug,
                PostCount = _unitOfWork.Posts.GetQueryable()
                    .Where(p => p.CategoryId == c.Id && p.Status == Domain.Entities.PostStatus.Published)
                    .Count()
            }).ToList();

            return Ok(categoryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { Error = "Eroare la preluarea categoriilor" });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategoryById(Guid id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            
            if (category == null)
            {
                return NotFound("Categorie negăsită");
            }

            var categoryDto = new
            {
                category.Id,
                category.Name,
                category.Description,
                category.Slug
            };

            return Ok(categoryDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category - CategoryId: {CategoryId}", id);
            return StatusCode(500, new { Error = "Eroare la preluarea categoriei" });
        }
    }
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetQueryable()
                .FirstOrDefaultAsync(c => c.Slug == slug);

            if (category == null)
                return NotFound("Categorie negăsită");

            var postCount = _unitOfWork.Posts.GetQueryable()
                .Where(p => p.CategoryId == category.Id && p.Status == PostStatus.Published)
                .Count();

            return Ok(new { category.Id, category.Name, category.Description, category.Slug, PostCount = postCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category - Slug: {Slug}", slug);
            return StatusCode(500, new { Error = "Eroare la preluarea categoriei" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            var existing = await _unitOfWork.Categories.GetQueryable().AnyAsync(c => c.Slug == slug);
            if (existing) return BadRequest(new { Error = "O categorie cu acest nume există deja" });

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                Slug = slug,
                Color = request.Color
            };

            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category created: {Name}", category.Name);
            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id },
                new { category.Id, category.Name, category.Description, category.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, new { Error = "Eroare la crearea categoriei" });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return NotFound("Categorie negăsită");

            category.Name = request.Name;
            category.Description = request.Description;
            category.Slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            category.Color = request.Color;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category updated: {Id}", id);
            return Ok(new { category.Id, category.Name, category.Description, category.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category - CategoryId: {Id}", id);
            return StatusCode(500, new { Error = "Eroare la actualizarea categoriei" });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
        try
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(id);
            if (category == null) return NotFound("Categorie negăsită");

            _unitOfWork.Categories.Remove(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Category deleted: {Id}", id);
            return Ok(new { Message = "Categoria a fost ștearsă" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category - CategoryId: {Id}", id);
            return StatusCode(500, new { Error = "Eroare la ștergerea categoriei" });
        }
    }
}

public class CategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
} 
