using StudyAuthApp.WebApi.Interfaces;

namespace StudyAuthApp.WebApi.AuthorizeHelpers
{
    public class AuthorizeMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthorizeMiddleware(RequestDelegate next)
            => _next = next;
        
        public async Task Invoke(HttpContext context,
                                ITokenService tokenService, 
                                IUserRepository userRepository)
        {
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();

            if(!string.IsNullOrEmpty(authorizationHeader)) 
            {
                var accessToken = authorizationHeader[7..];

                var id = tokenService.DecodeToken(accessToken, out bool hasTokenExpired);

                var user = await userRepository.GetUserById(id);

                if (user != null)
                    context.Items["User"] = user;
            }

            await _next(context);
        }
    }
}
