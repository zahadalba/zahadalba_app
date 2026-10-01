using zahadalba_backend.Data.Entities;
using zahadalba_backend.Services;

namespace zahadalba_backend.Features.Auth
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user, IDateTimeService dateTimeService);

        string GenerateRefreshToken();
    }
}
