using Blog.Application.Features.Posts.DTOs;
using Blog.Application.Features.Posts.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly ILogger<PostsController> _logger;

    public PostsController(IPostService postService, ILogger<PostsController> logger)
    {
        _postService = postService;
        _logger = logger;
    }

    [HttpPost]
    [Authorize(Roles = "Author,Admin")]
    [ProducesResponseType(typeof(PostResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                _logger.LogWarning("Model validation failed: {Errors}", string.Join(", ", errors));
                return BadRequest(new { Errors = errors });
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("KullanÄ±cÄ± kimliÄŸi bulunamadÄ±");
            }

            var post = await _postService.CreatePostAsync(request, userId);
            
            _logger.LogInformation("ğŸ“ Post created: {PostId} by {UserId}", post.Id, userId);
            
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new { Error = "Post oluÅŸturulurken hata oluÅŸtu" });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Author,Admin")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("KullanÄ±cÄ± kimliÄŸi bulunamadÄ±");
            }
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Post bulunamadÄ±");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Sadece post sahibi veya admin bu iÅŸlemi yapabilir");
            }

            var updatedPost = await _postService.UpdatePostAsync(id, request);
            
            _logger.LogInformation("âœï¸ Post updated: {PostId} by {UserId}", id, userId);
            
            return Ok(updatedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, new { Error = "Post gÃ¼ncellenirken hata oluÅŸtu" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Author,Admin")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeletePost(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("KullanÄ±cÄ± kimliÄŸi bulunamadÄ±");
            }
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Post bulunamadÄ±");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Sadece post sahibi veya admin bu iÅŸlemi yapabilir");
            }

            await _postService.DeletePostAsync(id);
            
            _logger.LogInformation("ğŸ—‘ï¸ Post deleted: {PostId} by {UserId}", id, userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, new { Error = "Post silinirken hata oluÅŸtu" });
        }
    }
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPostById(Guid id)
    {
        try
        {
            var post = await _postService.GetPostByIdAsync(id);
            
            if (post == null)
            {
                return NotFound("Post bulunamadÄ±");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post getirme hatasÄ± - PostId: {PostId}", id);
            return StatusCode(500, "Post getirilirken bir hata oluÅŸtu");
        }
    }

    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetPostBySlug(string slug)
    {
        try
        {
            var post = await _postService.GetPostBySlugAsync(slug);
            
            if (post == null)
            {
                return NotFound("Post bulunamadÄ±");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post getirme hatasÄ± - Slug: {Slug}", slug);
            return StatusCode(500, "Post getirilirken bir hata oluÅŸtu");
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PostResponse>), 200)]
    public async Task<IActionResult> GetPublishedPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.GetPublishedPostsAsync(page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post listesi getirme hatasÄ±");
            return StatusCode(500, "Post listesi getirilirken bir hata oluÅŸtu");
        }
    }

    [HttpGet("my-posts")]
    [Authorize]
    [ProducesResponseType(typeof(PagedResult<PostResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetMyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("KullanÄ±cÄ± kimliÄŸi bulunamadÄ±");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.GetUserPostsAsync(userId, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obÈ›inerea postÄƒrilor utilizatorului");
            return StatusCode(500, "A apÄƒrut o eroare la obÈ›inerea postÄƒrilor");
        }
    }

    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PagedResult<PostResponse>), 200)]
    public async Task<IActionResult> GetPostsByCategory(Guid categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.GetPostsByCategoryAsync(categoryId, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori postlarÄ± getirme hatasÄ± - CategoryId: {CategoryId}", categoryId);
            return StatusCode(500, "Kategori postlarÄ± getirilirken bir hata oluÅŸtu");
        }
    }

    [HttpGet("category/slug/{categorySlug}")]
    [ProducesResponseType(typeof(PagedResult<PostResponse>), 200)]
    public async Task<IActionResult> GetPostsByCategorySlug(string categorySlug, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.GetPostsByCategorySlugAsync(categorySlug, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kategori postlarÄ± getirme hatasÄ± - CategorySlug: {CategorySlug}", categorySlug);
            return StatusCode(500, "Kategori postlarÄ± getirilirken bir hata oluÅŸtu");
        }
    }

    [HttpGet("tag/{tagSlug}")]
    [ProducesResponseType(typeof(PagedResult<PostResponse>), 200)]
    public async Task<IActionResult> GetPostsByTag(string tagSlug, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.GetPostsByTagAsync(tagSlug, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tag postlarÄ± getirme hatasÄ± - TagSlug: {TagSlug}", tagSlug);
            return StatusCode(500, "Tag postlarÄ± getirilirken bir hata oluÅŸtu");
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(PagedResult<PostResponse>), 200)]
    public async Task<IActionResult> SearchPosts([FromQuery] string query, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Arama terimi boÅŸ olamaz");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.SearchPostsAsync(query, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Post arama hatasÄ± - Query: {Query}", query);
            return StatusCode(500, "Arama yapÄ±lÄ±rken bir hata oluÅŸtu");
        }
    }

    [HttpPost("{id}/publish")]
    [Authorize(Roles = "Author,Admin")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> PublishPost(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("KullanÄ±cÄ± kimliÄŸi bulunamadÄ±");
            }

            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Post bulunamadÄ±");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Sadece post sahibi veya admin bu iÅŸlemi yapabilir");
            }

            var publishedPost = await _postService.PublishPostAsync(id);
            
            _logger.LogInformation("ğŸ“¢ Post published: {PostId} by {UserId}", id, userId);
            
            return Ok(publishedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing post {PostId}", id);
            return StatusCode(500, new { Error = "Post yayÄ±nlanÄ±rken hata oluÅŸtu" });
        }
    }

    [HttpPost("{id}/unpublish")]
    [Authorize(Roles = "Author,Admin")]
    [ProducesResponseType(typeof(PostResponse), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UnpublishPost(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("KullanÄ±cÄ± kimliÄŸi bulunamadÄ±");
            }

            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Post bulunamadÄ±");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Sadece post sahibi veya admin bu iÅŸlemi yapabilir");
            }

            var unpublishedPost = await _postService.UnpublishPostAsync(id);
            
            _logger.LogInformation("ğŸ“ Post unpublished: {PostId} by {UserId}", id, userId);
            
            return Ok(unpublishedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing post {PostId}", id);
            return StatusCode(500, new { Error = "Post yayÄ±ndan kaldÄ±rÄ±lÄ±rken hata oluÅŸtu" });
        }
    }

    #region Helper Methods

    /// <summary>
    /// JWT token'dan user ID'yi al
    /// </summary>
    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Helper method: Current User Roles
    /// </summary>
    private List<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    #endregion
} 
