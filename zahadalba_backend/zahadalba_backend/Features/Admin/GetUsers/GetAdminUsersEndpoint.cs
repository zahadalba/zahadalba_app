using Microsoft.EntityFrameworkCore;
using zahadalba_backend.Data;

namespace zahadalba_backend.Features.Admin.GetUsers
{
    public static class GetAdminUsersEndpoint
    {
        public static void MapGetAdminUsersEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/admin/users", Handler).RequireAuthorization("AdminOnly");
        }

        private static async Task<IResult> Handler(AppDbContext db)
        {
            var users = await db.Users
                .Select(u => new { Id = u.Id.ToString(), u.FirstName, u.LastName, u.Age, u.Email, u.Phone, u.Role })
                .ToListAsync();

            return Results.Ok(users);
        }
    }
}
