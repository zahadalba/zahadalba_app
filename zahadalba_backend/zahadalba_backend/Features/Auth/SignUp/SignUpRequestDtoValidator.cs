using FluentValidation;

namespace zahadalba_backend.Features.Auth.SignUp
{
    public class SignUpRequestDtoValidator : AbstractValidator<SignUpRequestDto>
    {
        public SignUpRequestDtoValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Введите имя")
                .Length(2, 50).WithMessage("Имя должно содержать от 2 до 50 символов")
                .Matches(@"^\p{L}+(?:[ '-]\p{L}+)*$").WithMessage("Имя содержит недопустимые символы");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Введите фамилию")
                .Length(2, 50).WithMessage("Фамилия должна содержать от 2 до 50 символов")
                .Matches(@"^\p{L}+(?:[ '-]\p{L}+)*$").WithMessage("Фамилия содержит недопустимые символы");

            RuleFor(x => x.Age)
                .InclusiveBetween(14, 120).WithMessage("Возраст должен быть от 14 до 120 лет");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введите email")
                .EmailAddress().WithMessage("Введите корректный email");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Введите номер телефона")
                .Matches(@"^\+?[1-9]\d{9,14}$").WithMessage("Введите корректный номер телефона");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введите пароль")
                .Length(8, 64).WithMessage("Пароль должен содержать от 8 до 64 символов")
                .Matches("[A-Z]").WithMessage("Пароль должен содержать заглавную букву")
                .Matches("[a-z]").WithMessage("Пароль должен содержать строчную букву")
                .Matches(@"\d").WithMessage("Пароль должен содержать цифру")
                .Matches(@"[^A-Za-z0-9]").WithMessage("Пароль должен содержать специальный символ");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Подтвердите пароль")
                .Equal(x => x.Password).WithMessage("Пароли не совпадают");
        }
    }
}
