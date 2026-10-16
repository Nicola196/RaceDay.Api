using RaceDay.Api.Models;

namespace RaceDay.Api.Services;

public interface ITokenService
{
    string CreateToken(User user);
    DateTime GetExpiryUtc();
}