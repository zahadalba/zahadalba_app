using Microsoft.EntityFrameworkCore;
using zahadalba_backend.Data;
using zahadalba_backend.Services;

namespace zahadalba_backend.Features.Auth.SignOut
{
    public static class SignOutEndpoint
    {
        public static void MapSignOutEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/sign-out", Handler);
        }

        private static async Task<IResult> Handler(
            HttpContext context,
            IDateTimeService dateTimeService,
            AppDbContext db,
            ISha256HasherService sha256HasherService)
        {
            var refreshTokenData = context.Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshTokenData))
            {
                var existingRefreshToken = await db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == sha256HasherService.GetHash(refreshTokenData));
                if (existingRefreshToken is not null)
                {
                    existingRefreshToken.IsRevoked = true;
                    existingRefreshToken.RevokedAt = dateTimeService.UtcNow;
                    await db.SaveChangesAsync();
                }
            }
            return Results.Ok();
        }
    }
}
