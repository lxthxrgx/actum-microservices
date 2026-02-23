using System.Security.Claims;

namespace Gateway.Middleware
{
    public class UserIdMiddleware
    {
        private readonly RequestDelegate _next;

        public UserIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            context.Request.Headers.Remove("X-User-Id");
            context.Request.Headers.Remove("X-User-Role");
            context.Request.Headers.Remove("X-User-Company");

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var company = context.User.FindFirst("Company")?.Value;

                context.Request.Headers["X-User-Id"] = userId;
                context.Request.Headers["X-User-Role"] = role;
                context.Request.Headers["X-User-Company"] = company;
            }

            await _next(context);
        }
    }
}
