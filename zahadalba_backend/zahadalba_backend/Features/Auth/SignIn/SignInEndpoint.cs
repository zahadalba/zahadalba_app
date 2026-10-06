using FluentValidation;
using zahadalba_backend.Data;
using zahadalba_backend.Data.Entities;
using zahadalba_backend.Services;
using Microsoft.AspNetCore.Identity;

namespace zahadalba_backend.Features.Auth.SignIn
{
    public static class SignInEndpoint
    {
        public static void MapSignInEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/sign-in", Handle).RequireRateLimiting("signInLimit");
        }

        private static async Task<IResult> Handle(
            SignInRequestDto dto,
            IDateTimeService dateTimeService,
            AppDbContext dbContext,
            ITokenService tokenService,
            ISha256HasherService sha256HasherService,
            IPasswordHasher<User> passwordHasher,
            IValidator<SignInRequestDto> validator)
        {
            var normalizedDto = dto with
            {
                Email = dto.Email.Trim(),
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
                    title: "Ошибка авторизации",
                    detail: "Отправлены некорректные данные");
            }

            var user = dbContext.Users.FirstOrDefault(u => u.Email == normalizedDto.Email);
            if (user is null)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict,
                    title: "Ошибка авторизации",
                    detail: "Пользователь не найден");
            }

            var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, normalizedDto.Password);
            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Results.Problem(
                    statusCode: StatusCodes.Status409Conflict, 
                    title: "Ошибка авторизации",
                    detail: "Неверный email или пароль");
            }

            if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.PasswordHash = passwordHasher.HashPassword(user, normalizedDto.Password);
            }

            var accessToken = tokenService.GenerateAccessToken(user, dateTimeService);
            var refreshToken = tokenService.GenerateRefreshToken();

            dbContext.RefreshTokens.Add(new RefreshToken(
                userId: user.Id,
                tokenHash: sha256HasherService.GetHash(refreshToken),
                createdAt: dateTimeService.UtcNow,
                expiresAt: dateTimeService.UtcNow.AddDays(7),
                absoluteExpiresAt: dateTimeService.UtcNow.AddDays(30)));

            return Results.Ok(new
            {
                Id = user.Id.ToString(),
                user.FirstName,
                user.LastName,
                user.Age,
                user.Email,
                user.Phone,
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }
    }
}
