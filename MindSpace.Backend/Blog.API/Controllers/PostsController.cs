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
                return Unauthorized("Identificatorul utilizatorului nu a fost găsit");
            }

            var post = await _postService.CreatePostAsync(request, userId);
            
            _logger.LogInformation("ğŸ“ Post created: {PostId} by {UserId}", post.Id, userId);
            
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating post");
            return StatusCode(500, new { Error = "A apărut o eroare la crearea articolului" });
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
                return Unauthorized("Identificatorul utilizatorului nu a fost găsit");
            }
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Articolul nu a fost găsit");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Doar autorul articolului sau administratorul poate efectua această acțiune");
            }

            var updatedPost = await _postService.UpdatePostAsync(id, request);
            
            _logger.LogInformation("âœï¸ Post updated: {PostId} by {UserId}", id, userId);
            
            return Ok(updatedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating post {PostId}", id);
            return StatusCode(500, new { Error = "A apărut o eroare la actualizarea articolului" });
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
                return Unauthorized("Identificatorul utilizatorului nu a fost găsit");
            }
            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Articolul nu a fost găsit");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Doar autorul articolului sau administratorul poate efectua această acțiune");
            }

            await _postService.DeletePostAsync(id);
            
            _logger.LogInformation("ğŸ—‘ï¸ Post deleted: {PostId} by {UserId}", id, userId);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting post {PostId}", id);
            return StatusCode(500, new { Error = "A apărut o eroare la ștergerea articolului" });
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
                return NotFound("Articolul nu a fost găsit");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la preluarea articolului - PostId: {PostId}", id);
            return StatusCode(500, "A apărut o eroare la preluarea articolului");
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
                return NotFound("Articolul nu a fost găsit");
            }

            return Ok(post);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la preluarea articolului - Slug: {Slug}", slug);
            return StatusCode(500, "A apărut o eroare la preluarea articolului");
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
            _logger.LogError(ex, "Eroare la preluarea listei de articole");
            return StatusCode(500, "A apărut o eroare la preluarea listei de articole");
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
                return Unauthorized("Identificatorul utilizatorului nu a fost găsit");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.GetUserPostsAsync(userId, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la obținerea postărilor utilizatorului");
            return StatusCode(500, "A apărut o eroare la obținerea articolelor");
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
            _logger.LogError(ex, "Eroare la preluarea articolelor din categorie - CategoryId: {CategoryId}", categoryId);
            return StatusCode(500, "A apărut o eroare la preluarea articolelor din categorie");
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
            _logger.LogError(ex, "Eroare la preluarea articolelor din categorie - CategorySlug: {CategorySlug}", categorySlug);
            return StatusCode(500, "A apărut o eroare la preluarea articolelor din categorie");
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
            _logger.LogError(ex, "Eroare la preluarea articolelor cu eticheta - TagSlug: {TagSlug}", tagSlug);
            return StatusCode(500, "A apărut o eroare la preluarea articolelor cu eticheta");
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
                return BadRequest("Termenul de căutare nu poate fi gol");
            }

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _postService.SearchPostsAsync(query, page, pageSize);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Eroare la căutarea articolelor - Query: {Query}", query);
            return StatusCode(500, "A apărut o eroare la efectuarea căutării");
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
                return Unauthorized("Identificatorul utilizatorului nu a fost găsit");
            }

            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Articolul nu a fost găsit");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Doar autorul articolului sau administratorul poate efectua această acțiune");
            }

            var publishedPost = await _postService.PublishPostAsync(id);
            
            _logger.LogInformation("ğŸ“¢ Post published: {PostId} by {UserId}", id, userId);
            
            return Ok(publishedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing post {PostId}", id);
            return StatusCode(500, new { Error = "A apărut o eroare la publicarea articolului" });
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
                return Unauthorized("Identificatorul utilizatorului nu a fost găsit");
            }

            var existingPost = await _postService.GetPostByIdAsync(id);
            if (existingPost == null)
            {
                return NotFound("Articolul nu a fost găsit");
            }

            var userRoles = GetCurrentUserRoles();
            var isAdmin = userRoles.Contains("Admin");
            var isOwner = existingPost.Author.Id == userId;

            if (!isAdmin && !isOwner)
            {
                return Forbid("Doar autorul articolului sau administratorul poate efectua această acțiune");
            }

            var unpublishedPost = await _postService.UnpublishPostAsync(id);
            
            _logger.LogInformation("ğŸ“ Post unpublished: {PostId} by {UserId}", id, userId);
            
            return Ok(unpublishedPost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unpublishing post {PostId}", id);
            return StatusCode(500, new { Error = "A apărut o eroare la retragerea articolului" });
        }
    }

    #region Helper Methods

    /// <summary>
    /// Preluarea ID-ului utilizatorului din tokenul JWT
    /// </summary>
    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    /// <summary>
    /// Metodă ajutătoare: Rolurile utilizatorului curent
    /// </summary>
    private List<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    }

    #endregion
} 
