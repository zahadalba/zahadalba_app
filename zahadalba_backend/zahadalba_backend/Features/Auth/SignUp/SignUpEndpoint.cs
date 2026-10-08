using FluentValidation;
using Microsoft.AspNetCore.Identity;
using zahadalba_backend.Data;
using zahadalba_backend.Data.Entities;

namespace zahadalba_backend.Features.Auth.SignUp
{
    public static class SignUpEndpoint
    {
        public static void MapSignUpEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/sign-up", Handler);
        }

        private static async Task<IResult> Handler(
            SignUpRequestDto dto,
            IValidator<SignUpRequestDto> validator,
            IPasswordHasher<User> passwordHasher,
            AppDbContext db)
        {
            var normalizedDto = dto with
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = dto.Email.Trim(),
                Phone = dto.Phone.Trim(),
            };

            var validationResult = await validator.ValidateAsync(normalizedDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                    x => x.Key,
                    x => x.Select(x => x.ErrorMessage).ToArray());
                return Results.ValidationProblem(
                    errors: errors, 
                    statusCode: StatusCodes.Status400BadRequest, 
                    title: "Ошибка регистрации", 
                    detail: "Отправлены некорректные данные");
            }

            if (db.Users.Any(u => u.Email == normalizedDto.Email || u.Phone == normalizedDto.Phone))
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Ошибка регистрации",
                    detail: "Пользователь с таким email или номером телефона уже существует");
            }

            var user = new User
            {
                FirstName = normalizedDto.FirstName,
                LastName = normalizedDto.LastName,
                Age = normalizedDto.Age,
                Email = normalizedDto.Email,
                Phone = normalizedDto.Phone,
            };

            var passwordHash = passwordHasher.HashPassword(user, normalizedDto.Password);
            user.PasswordHash = passwordHash;

            db.Users.Add(user);
            await db.SaveChangesAsync();

            foreach (var u in db.Users)
            {
                Console.WriteLine($"{u.Id} {u.FirstName} {u.LastName} {u.Age} {u.Email} {u.Phone}");
            }

            return Results.Created();
        }
    }
}
