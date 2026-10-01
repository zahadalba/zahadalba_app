namespace zahadalba_backend.Features.Auth.SignUp
{
    public static class SignUpEndpoint
    {
        public static void MapSignUpEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/sign-up", Handler);
        }

        private static IResult Handler()
        {
            return Results.Ok();
        }
    }
}
