using Microsoft.EntityFrameworkCore;
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

        private static async Task<IResult> Handler(
            HttpContext context,
            IDateTimeService dateTimeService,
            AppDbContext db,
            ISha256HasherService sha256HasherService,
            ITokenService tokenService)
        {
            Console.WriteLine("refresh");
            var refreshTokenData = context.Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenData))
            {
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");
            }

            await using var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                var existingRefreshToken = await db.RefreshTokens
                    .FromSqlInterpolated($"""
                        SELECT *
                        FROM "RefreshTokens"
                        WHERE "TokenHash" = {sha256HasherService.GetHash(refreshTokenData)}
                        FOR UPDATE
                        """)
                    .SingleOrDefaultAsync();
                if (existingRefreshToken is null || existingRefreshToken.ExpiresAt <= dateTimeService.UtcNow || existingRefreshToken.AbsoluteExpiresAt <= dateTimeService.UtcNow)
                {
                    return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");
                }

                if (existingRefreshToken.IsRevoked)
                {
                    Console.WriteLine($"IsRevoked: {existingRefreshToken.IsRevoked}");
                    Console.WriteLine($"RevokedAt: {existingRefreshToken.RevokedAt:O}");
                    Console.WriteLine($"UtcNow: {dateTimeService.UtcNow:O}");
                    var timeSinceRevocation = dateTimeService.UtcNow - existingRefreshToken.RevokedAt;
                    if (timeSinceRevocation is not null && timeSinceRevocation.Value.TotalSeconds <= 10)
                    {
                        var retryUser = db.Users.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
                        if (retryUser is null)
                        {
                            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");
                        }

                        var retryAccessToken = tokenService.GenerateAccessToken(retryUser, dateTimeService);

                        return Results.Ok(new
                        {
                            Status = "Grace",
                            AccessToken = retryAccessToken,
                            RefreshToken = "",
                        });
                    }

                    foreach (var t in db.RefreshTokens.Where(t => t.UserId == existingRefreshToken.UserId))
                    {
                        t.IsRevoked = true;
                        t.RevokedAt = dateTimeService.UtcNow;
                    }

                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized: token reuse detected!");
                }

                var user = db.Users.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
                if (user is null)
                {
                    return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Unauthorized");
                }

                var accessToken = tokenService.GenerateAccessToken(user, dateTimeService);
                var refreshToken = tokenService.GenerateRefreshToken();

                var newExpiresAt = dateTimeService.UtcNow.AddDays(7);
                if (newExpiresAt > existingRefreshToken.AbsoluteExpiresAt)
                {
                    newExpiresAt = existingRefreshToken.AbsoluteExpiresAt;
                }

                db.RefreshTokens.Add(new RefreshToken(
                    userId: user.Id,
                    tokenHash: sha256HasherService.GetHash(refreshToken),
                    createdAt: dateTimeService.UtcNow,
                    expiresAt: newExpiresAt,
                    absoluteExpiresAt: existingRefreshToken.AbsoluteExpiresAt));

                existingRefreshToken.IsRevoked = true;
                existingRefreshToken.RevokedAt = dateTimeService.UtcNow;

                await db.SaveChangesAsync();

                Console.WriteLine("-------------------------");
                foreach (var t in db.RefreshTokens)
                {
                    Console.WriteLine($"TokenHash: {t.TokenHash} | CreatedAt: {t.CreatedAt} | RevokedAt: {t.RevokedAt}");
                }

                await transaction.CommitAsync();

                return Results.Ok(new
                {
                    Status = "Refreshed",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Results.Problem();
            }
        }
    }
}
