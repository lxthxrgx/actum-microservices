namespace Auth.service
{
    public class httpOnly
    {
        public void SetHttpOnlyCookie(string accessToken, string refreshToken, HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(14)
            };

            response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SameSite = SameSiteMode.Strict
            });

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
