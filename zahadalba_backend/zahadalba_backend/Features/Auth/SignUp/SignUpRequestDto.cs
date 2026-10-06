namespace zahadalba_backend.Features.Auth.SignUp
{
    public record SignUpRequestDto(
        string FirstName,
        string LastName,
        int Age,
        string Email,
        string Phone,
        string Password,
        string ConfirmPassword);
}
