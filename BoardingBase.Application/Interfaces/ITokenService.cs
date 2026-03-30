
namespace BoardingBase.Application.Interfaces;

public interface ITokenService
{
    Task<string> CreateAccessToken(AuthUser user, IList<string> roles);
    string CreateRefreshToken();
}