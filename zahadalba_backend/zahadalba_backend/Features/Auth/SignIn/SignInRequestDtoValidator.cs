using FluentValidation;
using zahadalba_backend.Features.Auth.SignUp;

namespace zahadalba_backend.Features.Auth.SignIn
{
    public class SignInRequestDtoValidator : AbstractValidator<SignInRequestDto>
    {
        public SignInRequestDtoValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введите email")
                .EmailAddress().WithMessage("Введите корректный email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введите пароль");
        }
    }
}
