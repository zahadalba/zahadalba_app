using Microsoft.EntityFrameworkCore;
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

        private static async Task<IResult> Handler(
            HttpContext context,
            AppDbContext db)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Не авторизован",
                    detail: "Не удалось получить ID пользователя");
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status401Unauthorized,
                    title: "Не авторизован",
                    detail: "Пользователь не найден");
            }

            return Results.Ok(new
            {
                Id = user.Id.ToString(),
                user.FirstName,
                user.LastName,
                user.Age,
                user.Email,
                user.Phone,
                user.Role,
            });
        }
    }
}
