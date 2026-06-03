namespace Blog.Application.Common.Interfaces;

public interface ITokenBlacklistService
{
    void RevokeToken(string jti, DateTime expiry);
    bool IsRevoked(string jti);
}
