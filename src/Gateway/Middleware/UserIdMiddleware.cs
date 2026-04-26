using System.Security.Claims;

namespace Gateway.Middleware
{
    // Добавляет X-User-* хедеры к запросам которые идут в downstream сервисы.
    // Сервисы читают эти хедеры вместо того чтобы самим валидировать JWT.
    public class UserIdMiddleware
    {
        private readonly RequestDelegate _next;

        public UserIdMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            // Очищаем — никто снаружи не должен подделать эти хедеры
            context.Request.Headers.Remove("X-User-Id");
            context.Request.Headers.Remove("X-User-Role");
            context.Request.Headers.Remove("X-User-Company");

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId  = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role    = context.User.FindFirst(ClaimTypes.Role)?.Value;
                var company = context.User.FindFirst("Company")?.Value;

                if (!string.IsNullOrEmpty(userId))
                    context.Request.Headers["X-User-Id"] = userId;

                if (!string.IsNullOrEmpty(role))
                    context.Request.Headers["X-User-Role"] = role;

                if (!string.IsNullOrEmpty(company))
                    context.Request.Headers["X-User-Company"] = company;
            }

            await _next(context);
        }
    }
}
