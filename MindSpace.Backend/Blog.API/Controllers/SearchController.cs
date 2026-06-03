using Blog.Application.Common.Interfaces;
using Blog.Application.Common.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog.API.Controllers;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly IElasticsearchService _elasticsearchService;
    private readonly ILogger<SearchController> _logger;

    public SearchController(
        IElasticsearchService elasticsearchService,
        ILogger<SearchController> logger)
    {
        _elasticsearchService = elasticsearchService;
        _logger = logger;
    }

    [HttpPost("posts")]
    [ProducesResponseType(typeof(SearchResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> SearchPosts([FromBody] SearchRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest(new { Message = "Search request cannot be null" });
            }

            if (request.Page < 1)
                request.Page = 1;

            if (request.PageSize < 1 || request.PageSize > 100)
                request.PageSize = 20;

            var response = await _elasticsearchService.SearchPostsAsync(request);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching posts");
            return StatusCode(500, new { Message = "A apărut o eroare în timpul căutării" });
        }
    }

    [HttpGet("quick")]
    [ProducesResponseType(typeof(SearchResponse), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> QuickSearch(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new SearchResponse
                {
                    Results = new List<PostSearchResult>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = 0,
                    SearchTime = TimeSpan.Zero
                });
            }

            var request = new SearchRequest
            {
                Query = q,
                Page = page,
                PageSize = pageSize,
                SortBy = SearchSortBy.Relevance,
                HighlightResults = true
            };

            var response = await _elasticsearchService.SearchPostsAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in quick search");
            return StatusCode(500, new { Message = "A apărut o eroare în timpul căutării rapide" });
        }
    }

    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(List<SearchSuggestion>), 200)]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string q,
        [FromQuery] int maxSuggestions = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return Ok(new List<SearchSuggestion>());
            }

            var suggestions = await _elasticsearchService.GetSuggestionsAsync(q, maxSuggestions);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting search suggestions");
            return Ok(new List<SearchSuggestion>());
        }
    }

    [HttpGet("similar/{postId:guid}")]
    [ProducesResponseType(typeof(List<PostSearchResult>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetSimilarPosts(
        Guid postId,
        [FromQuery] int maxResults = 5)
    {
        try
        {
            var similarPosts = await _elasticsearchService.GetSimilarPostsAsync(postId, maxResults);
            return Ok(similarPosts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar posts for {PostId}", postId);
            return StatusCode(500, new { Message = "A apărut o eroare la preluarea articolelor similare" });
        }
    }

    [HttpGet("popular")]
    [ProducesResponseType(typeof(List<PopularSearch>), 200)]
    public async Task<IActionResult> GetPopularSearches([FromQuery] int maxResults = 10)
    {
        try
        {
            var popularSearches = await _elasticsearchService.GetPopularSearchesAsync(maxResults);
            return Ok(popularSearches);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular searches");
            return StatusCode(500, new { Message = "A apărut o eroare la preluarea căutărilor populare" });
        }
    }

    [HttpGet("health")]
    [ProducesResponseType(200)]
    [ProducesResponseType(503)]
    public async Task<IActionResult> GetSearchHealth()
    {
        try
        {
            var isHealthy = await _elasticsearchService.IsHealthyAsync();
            var stats = await _elasticsearchService.GetIndexStatsAsync();

            var healthInfo = new
            {
                IsHealthy = isHealthy,
                Status = isHealthy ? "healthy" : "unhealthy",
                Timestamp = DateTime.UtcNow,
                Statistics = stats
            };

            return isHealthy ? Ok(healthInfo) : StatusCode(503, healthInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking search health");
            return StatusCode(503, new 
            { 
                IsHealthy = false,
                Status = "error",
                Message = "Verificarea stării motorului de căutare a eșuat",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    [HttpPost("reindex")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ReindexPosts()
    {
        try
        {
            await _elasticsearchService.ReindexAllPostsAsync();

            return Ok(new 
            { 
                Message = "✅ Re-indexing started successfully",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error re-indexing posts");
            return StatusCode(500, new { Message = "A apărut o eroare în timpul reindexării" });
        }
    }

    [HttpPost("test-data")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> AddTestData()
    {
        try
        {
            var testPosts = GenerateTestSearchData();
            await _elasticsearchService.BulkIndexPostsAsync(testPosts);

            return Ok(new 
            { 
                Message = $"✅ {testPosts.Count} test posts added to search index",
                TestPosts = testPosts.Select(p => new { p.Id, p.Title }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding test data");
            return StatusCode(500, new { Message = "A apărut o eroare la adăugarea datelor de test" });
        }
    }

    #region Helper Methods

    private List<PostSearchDocument> GenerateTestSearchData()
    {
        var categories = new[] { "Tehnologie", "Dezvoltare Software", "AI", "Web Development", "Mobile" };
        var tags = new[] { "react", "javascript", "typescript", "csharp", "dotnet", "api", "frontend", "backend" };
        var authors = new[] { "John Doe", "Jane Smith", "Andrei Popescu", "Elena Ionescu", "Mihai Radu" };

        var testPosts = new List<PostSearchDocument>();

        for (int i = 1; i <= 20; i++)
        {
            var category = categories[i % categories.Length];
            var author = authors[i % authors.Length];
            var postTags = tags.OrderBy(x => Guid.NewGuid()).Take(3).ToList();

            testPosts.Add(new PostSearchDocument
            {
                Id = Guid.NewGuid(),
                Title = $"Articol de Test #{i}: Ghid despre {category}",
                Content = $"Acesta este un articol de test. Conține informații detaliate despre {category}. " +
                         $"Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                         $"Acest articol acoperă subiectele {string.Join(", ", postTags)}.",
                Excerpt = $"Rezumat și sfaturi utile despre {category}",
                Slug = $"test-post-{i}-{category.ToLowerInvariant().Replace(" ", "-")}",
                AuthorId = Guid.NewGuid().ToString(),
                AuthorName = author,
                AuthorUserName = author.ToLowerInvariant().Replace(" ", "."),
                CategoryId = Guid.NewGuid(),
                CategoryName = category,
                Tags = postTags,
                PublishedAt = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow.AddDays(-i - 1),
                UpdatedAt = DateTime.UtcNow.AddDays(-i + 1),
                ViewCount = Random.Shared.Next(10, 1000),
                LikeCount = Random.Shared.Next(1, 50),
                CommentCount = Random.Shared.Next(0, 20),
                ReadTimeMinutes = Random.Shared.Next(2, 15),
                Status = "Published",
                MetaDescription = $"Ghid detaliat despre {category}",
                MetaKeywords = string.Join(", ", postTags),
                FeaturedImageUrl = $"https://picsum.photos/800/400?random={i}"
            });
        }

        return testPosts;
    }

    #endregion
} 
