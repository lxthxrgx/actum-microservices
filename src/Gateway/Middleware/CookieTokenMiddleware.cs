namespace Gateway.Middleware
{
    public class CookieTokenMiddleware
    {
        private readonly RequestDelegate _next;

        // Пути без проверки токена
        private static readonly string[] _publicPaths =
        [
            "/api/auth/sign-in",
            "/api/auth/sign-up",
        ];

        public CookieTokenMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            var path   = context.Request.Path.Value ?? "";
            var method = context.Request.Method;

            // Перехватываем sign-in чтобы установить куки из ответа Auth сервиса
            if (path.StartsWith("/api/auth/sign-in", StringComparison.OrdinalIgnoreCase) && method == "POST")
            {
                var originalBody = context.Response.Body;
                using var buffer = new MemoryStream();
                context.Response.Body = buffer;

                await _next(context);

                buffer.Position = 0;
                var body = await new StreamReader(buffer).ReadToEndAsync();

                if (context.Response.StatusCode == 200)
                {
                    try
                    {
                        var json = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(body);

                        if (json.TryGetProperty("accessToken", out var at) &&
                            json.TryGetProperty("refreshToken", out var rt))
                        {
                            context.Response.Cookies.Append("accessToken", at.GetString()!, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure   = false,  // true на проде
                                SameSite = SameSiteMode.Lax,
                                Expires  = DateTimeOffset.UtcNow.AddMinutes(15)
                            });

                            context.Response.Cookies.Append("refreshToken", rt.GetString()!, new CookieOptions
                            {
                                HttpOnly = true,
                                Secure   = false,
                                SameSite = SameSiteMode.Lax,
                                Expires  = DateTimeOffset.UtcNow.AddDays(14)
                            });
                        }
                    }
                    catch { }
                }

                buffer.Position = 0;
                context.Response.Body = originalBody;
                await buffer.CopyToAsync(originalBody);
                return;
            }

            // Публичные пути — пропускаем
            if (_publicPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Защищённые пути — проверяем куку
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                var accessToken  = context.Request.Cookies["accessToken"];
                var refreshToken = context.Request.Cookies["refreshToken"];

                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Добавляем токен в хедер — для UseAuthentication и Ocelot
                    context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
                }
                else if (!string.IsNullOrEmpty(refreshToken))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "Token expired", code = "TOKEN_EXPIRED" });
                    return;
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsJsonAsync(new { message = "Unauthorized", code = "NO_TOKEN" });
                    return;
                }
            }

            await _next(context);
        }
    }
}
