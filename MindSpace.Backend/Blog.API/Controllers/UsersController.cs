using Blog.Application.Common.Interfaces;
using Blog.Application.Features.Notifications.Interfaces;
using Blog.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ILogger<UsersController> _logger;
    private readonly UserManager<User> _userManager;

    public UsersController(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ILogger<UsersController> logger,
        UserManager<User> userManager)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _logger = logger;
        _userManager = userManager;
    }

    [HttpGet]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            var totalCount = allUsers.Count();
            var pagedUsers = allUsers
                .OrderByDescending(u => u.FollowerCount)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var users = new List<object>();
            foreach (var u in pagedUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                users.Add(new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.FirstName,
                    u.LastName,
                    u.Bio,
                    u.ProfileImageUrl,
                    u.FollowerCount,
                    u.FollowingCount,
                    u.IsVerified,
                    Roles = roles
                });
            }

            return Ok(new { users, totalCount });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users list");
            return StatusCode(500, new { Error = "A apărut o eroare la preluarea utilizatorilor" });
        }
    }

    [HttpGet("{userName}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserProfile(string userName)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(userName);
            if (user == null)
                return NotFound("Utilizatorul nu a fost găsit");

            var recentPosts = await _unitOfWork.Posts.FindAsync(p => 
                p.AuthorId == user.Id && p.Status == PostStatus.Published);
            var recentPostsList = recentPosts
                .OrderByDescending(p => p.PublishedAt)
                .Take(5)
                .ToList();

            var currentUserId = GetCurrentUserId();
            var isFollowing = false;

            if (!string.IsNullOrEmpty(currentUserId) && currentUserId != user.Id)
            {
                isFollowing = await _unitOfWork.Users.IsFollowingAsync(currentUserId, user.Id);
            }

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Bio,
                user.ProfileImageUrl,
                user.Website,
                user.TwitterHandle,
                user.LinkedInUrl,
                user.JoinDate,
                user.FollowerCount,
                user.FollowingCount,
                user.IsVerified,
                IsFollowing = isFollowing,
                RecentPosts = recentPostsList.Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Slug,
                    p.Excerpt,
                    p.PublishedAt,
                    p.ViewCount,
                    p.LikeCount,
                    p.CommentCount
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user profile for {UserName}", userName);
            return StatusCode(500, new { Error = "A apărut o eroare la preluarea profilului de utilizator" });
        }
    }

    [HttpPost("{userName}/follow")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ToggleFollow(string userName)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (string.IsNullOrEmpty(currentUserId))
                return Unauthorized();

            var userToFollow = await _unitOfWork.Users.GetByUserNameAsync(userName);
            if (userToFollow == null)
                return NotFound("Utilizatorul nu a fost găsit");

            // Kendini takip etmeye Ã§alÄ±ÅŸÄ±yorsa hata ver
            if (currentUserId == userToFollow.Id)
                return BadRequest("Nu vă puteți urmări pe dvs. înșivă");

            var isFollowing = await _unitOfWork.Users.IsFollowingAsync(currentUserId, userToFollow.Id);

            if (isFollowing)
            {
                // Unfollow
                await _unitOfWork.Users.UnfollowUserAsync(currentUserId, userToFollow.Id);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("User unfollowed: {FollowerId} unfollowed {FollowingId}", 
                    currentUserId, userToFollow.Id);

                return Ok(new { 
                    Message = "Abonarea a fost anulată",
                    IsFollowing = false
                });
            }
            else
            {
                // Follow
                await _unitOfWork.Users.FollowUserAsync(currentUserId, userToFollow.Id);
                await _unitOfWork.SaveChangesAsync();

                // Send notification
                await _notificationService.SendNewFollowerNotificationAsync(currentUserId, userToFollow.Id);

                _logger.LogInformation("User followed: {FollowerId} followed {FollowingId}", 
                    currentUserId, userToFollow.Id);

                return Ok(new { 
                    Message = "Utilizatorul este acum urmărit",
                    IsFollowing = true
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling follow for user {UserName}", userName);
            return StatusCode(500, new { Error = "A apărut o eroare în timpul procesului de urmărire" });
        }
    }
    [HttpGet("{userName}/followers")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFollowers(string userName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(userName);
            if (user == null)
                return NotFound("Utilizatorul nu a fost găsit");

            var followers = await _unitOfWork.Users.GetFollowersAsync(user.Id);
            var totalCount = followers.Count();
            var pagedFollowers = followers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var followerResponses = pagedFollowers.Select(f => new
            {
                f.Id,
                f.UserName,
                f.FirstName,
                f.LastName,
                f.Bio,
                f.ProfileImageUrl,
                f.FollowerCount,
                f.FollowingCount,
                f.IsVerified
            });

            return Ok(new
            {
                Followers = followerResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting followers for user {UserName}", userName);
            return StatusCode(500, new { Error = "A apărut o eroare la preluarea urmăritorilor" });
        }
    }

    [HttpGet("{userName}/following")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFollowing(string userName, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(userName);
            if (user == null)
                return NotFound("Utilizatorul nu a fost găsit");

            var following = await _unitOfWork.Users.GetFollowingAsync(user.Id);
            var totalCount = following.Count();
            var pagedFollowing = following
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var followingResponses = pagedFollowing.Select(f => new
            {
                f.Id,
                f.UserName,
                f.FirstName,
                f.LastName,
                f.Bio,
                f.ProfileImageUrl,
                f.FollowerCount,
                f.FollowingCount,
                f.IsVerified
            });

            return Ok(new
            {
                Following = followingResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting following for user {UserName}", userName);
            return StatusCode(500, new { Error = "A apărut o eroare la preluarea persoanelor urmărite" });
        }
    }
    [HttpGet("{userName}/posts")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetUserPosts(string userName, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUserNameAsync(userName);
            if (user == null)
                return NotFound("Utilizatorul nu a fost găsit");

            var posts = await _unitOfWork.Posts.FindAsync(p => 
                p.AuthorId == user.Id && p.Status == PostStatus.Published);
            var totalCount = posts.Count();
            var pagedPosts = posts
                .OrderByDescending(p => p.PublishedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var postResponses = pagedPosts.Select(p => new
            {
                p.Id,
                p.Title,
                p.Slug,
                p.Excerpt,
                p.FeaturedImageUrl,
                p.PublishedAt,
                p.ViewCount,
                p.LikeCount,
                p.CommentCount,
                p.ReadTimeMinutes,
                Category = p.Category != null ? new
                {
                    p.Category.Id,
                    p.Category.Name,
                    p.Category.Slug
                } : null,
                Tags = p.PostTags?.Select(pt => new
                {
                    pt.Tag.Id,
                    pt.Tag.Name,
                    pt.Tag.Slug
                }).ToList()
            });

            return Ok(new
            {
                Posts = postResponses,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting posts for user {UserName}", userName);
            return StatusCode(500, new { Error = "A apărut o eroare la preluarea articolelor" });
        }
    }

    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                return NotFound("Utilizatorul nu a fost găsit");

            user.FirstName = request.FirstName ?? user.FirstName;
            user.LastName = request.LastName ?? user.LastName;
            user.Bio = request.Bio ?? user.Bio;
            user.ProfileImageUrl = request.ProfileImageUrl ?? user.ProfileImageUrl;
            user.Website = request.Website ?? user.Website;
            user.TwitterHandle = request.TwitterHandle ?? user.TwitterHandle;
            user.LinkedInUrl = request.LinkedInUrl ?? user.LinkedInUrl;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User profile updated: {UserId}", userId);

            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Bio,
                user.ProfileImageUrl,
                user.Website,
                user.TwitterHandle,
                user.LinkedInUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", GetCurrentUserId());
            return StatusCode(500, new { Error = "A apărut o eroare la actualizarea profilului" });
        }
    }

    #region Helper Methods

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    #endregion
}

public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Website { get; set; }
    public string? TwitterHandle { get; set; }
    public string? LinkedInUrl { get; set; }
}
