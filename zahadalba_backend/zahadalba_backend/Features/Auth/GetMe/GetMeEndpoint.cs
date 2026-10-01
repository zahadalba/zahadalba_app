using System.Security.Claims;
using zahadalba_backend.Data;

namespace zahadalba_backend.Features.Auth.GetMe
{
    public static class GetMeEndpoint
    {
        public static void MapGetMeEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/auth/me", Handler).RequireAuthorization();
        }

        private static IResult Handler(
            HttpContext context,
            AppDbContext dbContext)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Results.Problem(statusCode: 401, title: "Unauthorized");
            }

            var user = dbContext.Users.FirstOrDefault(u => u.Id.ToString() == userId);
            if (user is null)
            {
                return Results.Problem(statusCode: 401, title: "Unauthorized");
            }

            return Results.Ok(new { Id = user.Id.ToString(), user.Email });
        }
    }
}
