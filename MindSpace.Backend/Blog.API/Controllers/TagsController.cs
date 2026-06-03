using Blog.Application.Common.Interfaces;
using Blog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blog.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TagsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TagsController> _logger;

    public TagsController(IUnitOfWork unitOfWork, ILogger<TagsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetTags()
    {
        try
        {
            var tags = await _unitOfWork.Tags.GetAllAsync();
            
            var tagDtos = tags.Select(t => new
            {
                t.Id,
                t.Name,
                t.Description,
                t.Slug,
                PostCount = _unitOfWork.Posts.GetQueryable()
                    .Where(p => p.PostTags.Any(pt => pt.TagId == t.Id) && p.Status == Domain.Entities.PostStatus.Published)
                    .Count()
            }).ToList();

            return Ok(tagDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiketler getirilirken hata oluştu");
            return StatusCode(500, new { Error = "Etiketler getirilirken hata oluştu" });
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTagById(Guid id)
    {
        try
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            
            if (tag == null)
            {
                return NotFound("Etiket bulunamadı");
            }

            var tagDto = new
            {
                tag.Id,
                tag.Name,
                tag.Description,
                tag.Slug
            };

            return Ok(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket getirilirken hata oluştu - TagId: {TagId}", id);
            return StatusCode(500, new { Error = "Etiket getirilirken hata oluştu" });
        }
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetTagBySlug(string slug)
    {
        try
        {
            var tag = await _unitOfWork.Tags.GetQueryable()
                .FirstOrDefaultAsync(t => t.Slug == slug);
            
            if (tag == null)
            {
                return NotFound("Etiket bulunamadı");
            }

            var postCount = _unitOfWork.Posts.GetQueryable()
                .Where(p => p.PostTags.Any(pt => pt.TagId == tag.Id) && p.Status == Domain.Entities.PostStatus.Published)
                .Count();

            var tagDto = new
            {
                tag.Id,
                tag.Name,
                tag.Description,
                tag.Slug,
                PostCount = postCount
            };

            return Ok(tagDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket getirilirken hata oluştu - Slug: {Slug}", slug);
            return StatusCode(500, new { Error = "Etiket getirilirken hata oluştu" });
        }
    }

    [HttpGet("popular")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetPopularTags([FromQuery] int count = 10)
    {
        try
        {
            var popularTags = await _unitOfWork.Tags.GetQueryable()
                .OrderByDescending(t => t.PostTags.Count)
                .Take(count)
                .Select(t => new { t.Id, t.Name, t.Slug, PostCount = t.PostTags.Count })
                .ToListAsync();

            return Ok(popularTags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Popüler etiketler getirilirken hata oluştu");
            return StatusCode(500, new { Error = "Popüler etiketler getirilirken hata oluştu" });
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateTag([FromBody] TagRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            var existing = await _unitOfWork.Tags.GetQueryable().AnyAsync(t => t.Slug == slug);
            if (existing) return BadRequest(new { Error = "Bu isimde bir etiket zaten var" });

            var tag = new Tag { Name = request.Name, Description = request.Description, Slug = slug, Color = request.Color };
            await _unitOfWork.Tags.AddAsync(tag);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Etiket oluşturuldu: {Name}", tag.Name);
            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id },
                new { tag.Id, tag.Name, tag.Description, tag.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket oluşturulurken hata oluştu");
            return StatusCode(500, new { Error = "Etiket oluşturulurken hata oluştu" });
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] TagRequest request)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) return NotFound("Etiket bulunamadı");

            tag.Name = request.Name;
            tag.Description = request.Description;
            tag.Slug = request.Name.ToLowerInvariant().Replace(" ", "-");
            tag.Color = request.Color;

            _unitOfWork.Tags.Update(tag);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { tag.Id, tag.Name, tag.Description, tag.Slug });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket güncellenirken hata oluştu - TagId: {Id}", id);
            return StatusCode(500, new { Error = "Etiket güncellenirken hata oluştu" });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        try
        {
            var tag = await _unitOfWork.Tags.GetByIdAsync(id);
            if (tag == null) return NotFound("Etiket bulunamadı");

            _unitOfWork.Tags.Remove(tag);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Etiket silindi: {Id}", id);
            return Ok(new { Message = "Etiket silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Etiket silinirken hata oluştu - TagId: {Id}", id);
            return StatusCode(500, new { Error = "Etiket silinirken hata oluştu" });
        }
    }
}

public class TagRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
} 
