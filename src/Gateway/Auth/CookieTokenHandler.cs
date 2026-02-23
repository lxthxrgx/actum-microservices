namespace Gateway.Auth
{
    public class CookieTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public CookieTokenMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var accessToken = context.Request.Cookies["accessToken"];
                var refreshToken = context.Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
                }
                else if (!string.IsNullOrEmpty(refreshToken))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = "Token expired",
                        code = "TOKEN_EXPIRED"
                    });
                    return;
                }
            }

            await _next(context);
        }
    }
}
