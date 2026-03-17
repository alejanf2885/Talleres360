namespace Talleres360.Extensions
{
    public static class CookieExtensions
    {
        public static void AppendRefreshTokenCookie(this HttpResponse response, string token)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true, 
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddDays(7)
            };

            response.Cookies.Append("refreshToken", token, cookieOptions);
        }
    }
}