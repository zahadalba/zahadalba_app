using zahadalba_backend.Data;
using zahadalba_backend.Data.Entities;
using zahadalba_backend.Services;

namespace zahadalba_backend.Features.Auth.Refresh
{
    public static class RefreshEndpoint
    {
        public static void MapRefreshEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/refresh", Handler);
        }

        private static IResult Handler(
            HttpContext context,
            IDateTimeService dateTimeService,
            AppDbContext dbContext,
            ISha256HasherService sha256HasherService,
            ITokenService tokenService)
        {
            var refreshTokenData = context.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenData))
            {
                return Results.Problem(statusCode: 401, title: "Unauthorized");
            }

            var existingRefreshToken = dbContext.RefreshTokens.FirstOrDefault(t => t.TokenHash == sha256HasherService.GetHash(refreshTokenData));
            if (existingRefreshToken is null || existingRefreshToken.ExpiresAt <= dateTimeService.UtcNow || existingRefreshToken.AbsoluteExpiresAt <= dateTimeService.UtcNow)
            {
                return Results.Problem(statusCode: 401, title: "Unauthorized");
            }

            if (existingRefreshToken.IsRevoked)
            {
                var timeSinceRevocation = dateTimeService.UtcNow - existingRefreshToken.RevokedAt;
                if (timeSinceRevocation is not null && timeSinceRevocation.Value.TotalSeconds <= 10)
                {
                    var retryUser = dbContext.Users.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
                    if (retryUser is null)
                    {
                        return Results.Problem(statusCode: 401, title: "Unauthorized");
                    }

                    var retryAccessToken = tokenService.GenerateAccessToken(retryUser, dateTimeService);
                    var retryRefreshToken = tokenService.GenerateRefreshToken();

                    dbContext.RefreshTokens.Add(new RefreshToken(
                        userId: retryUser.Id,
                        tokenHash: sha256HasherService.GetHash(retryRefreshToken),
                        createdAt: dateTimeService.UtcNow,
                        expiresAt: existingRefreshToken.ExpiresAt,
                        absoluteExpiresAt: existingRefreshToken.AbsoluteExpiresAt));

                    return Results.Ok(new
                    {
                        AccessToken = retryAccessToken,
                        RefreshToken = retryRefreshToken
                    });
                }

                foreach (var t in dbContext.RefreshTokens.Where(t => t.UserId == existingRefreshToken.UserId))
                {
                    t.IsRevoked = true;
                    t.RevokedAt = dateTimeService.UtcNow;
                }

                return Results.Problem(statusCode: 401, title: "Unauthorized: token reuse detected!");
            }

            var user = dbContext.Users.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
            if (user is null)
            {
                return Results.Problem(statusCode: 401, title: "Unauthorized");
            }

            var accessToken = tokenService.GenerateAccessToken(user, dateTimeService);
            var refreshToken = tokenService.GenerateRefreshToken();

            var newExpiresAt = dateTimeService.UtcNow.AddDays(7);
            if (newExpiresAt > existingRefreshToken.AbsoluteExpiresAt)
            {
                newExpiresAt = existingRefreshToken.AbsoluteExpiresAt;
            }

            dbContext.RefreshTokens.Add(new RefreshToken(
                userId: user.Id,
                tokenHash: sha256HasherService.GetHash(refreshToken),
                createdAt: dateTimeService.UtcNow,
                expiresAt: newExpiresAt,
                absoluteExpiresAt: existingRefreshToken.AbsoluteExpiresAt));

            existingRefreshToken.IsRevoked = true;
            existingRefreshToken.RevokedAt = dateTimeService.UtcNow;

            foreach (var t in dbContext.RefreshTokens)
            {
                Console.WriteLine($"{t.CreatedAt} | {t.RevokedAt}");
            }

            return Results.Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });

        }
    }
}
