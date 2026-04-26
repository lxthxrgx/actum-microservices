namespace Auth.service
{
    public class httpOnly
    {
        public void SetHttpOnlyCookie(string accessToken, string refreshToken, HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(14)
            };

            response.Cookies.Append("accessToken", accessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Expires = DateTime.UtcNow.AddMinutes(15),
                SameSite = SameSiteMode.Lax
            });

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }
}
