using Blog.Application.Common.Interfaces;
using System.Collections.Concurrent;

namespace Blog.Infrastructure.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    // jti -> expiry
    private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

    public void RevokeToken(string jti, DateTime expiry)
    {
        _revokedTokens[jti] = expiry;
        CleanupExpired();
    }

    public bool IsRevoked(string jti) =>
        _revokedTokens.TryGetValue(jti, out var expiry) && expiry > DateTime.UtcNow;

    private void CleanupExpired()
    {
        var expired = _revokedTokens.Where(kv => kv.Value <= DateTime.UtcNow).Select(kv => kv.Key).ToList();
        foreach (var key in expired)
            _revokedTokens.TryRemove(key, out _);
    }
}
