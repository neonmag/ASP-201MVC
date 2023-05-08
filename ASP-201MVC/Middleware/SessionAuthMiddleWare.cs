using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace ASP_201MVC.Middleware
{
    public class SessionAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context,
            ILogger<SessionAuthMiddleware> logger,
            DataContext dataContext)
        {
            //logger.LogInformation("SessionAuthMiddleware works");
            String? userId = context.Session.GetString("authUserId");
            if (userId is not null)
            {
                try
                {
                    User? authUser =
                    dataContext.Users.Find(Guid.Parse(userId));
                    if (authUser is not null)
                    {
                        /* Передача сведений о пользователе путем ссылки
                        * на объект-сущность (Entity) повышает сцепление
                        * (зависимость от реализаций), а также распространяющая сведения
                        * о "технической" сущности User, необходимой для ORM, на
                        * весь проект, где требуется авторизация.
                        * Для унификации основывает механизм "утверждений" (Claims).
                        * При аутентификации пользователю задаются определенные
                        * Claims, а при авторизации проверяется наличие
                        * нужных из них (например, возраст, пол, тлф).
                        */
                        context.Items.Add("authUser", authUser);
                        Claim[] claims = new Claim[]
                        {
                            new Claim(ClaimTypes.Sid, userId),
                            new Claim(ClaimTypes.Name, authUser.RealName),
                            new Claim(ClaimTypes.NameIdentifier, authUser.Login),
                            new Claim(ClaimTypes.UserData, authUser.Avatar ?? String.Empty),
                            new Claim(ClaimTypes.Email, authUser.EmailCode ?? String.Empty)
                        };
                        // создаем владельца (Principal) с этими утверждениями
                        var principal = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                claims,
                                nameof(SessionAuthMiddleware)));
                        // В HttpContext есть встроенное поле User с типом ClaimsPrincipal
                        // Установка его позволит использовать механизм ASP авторизации
                        context.User = principal;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "SessionAuthMiddleware");
                }
            }
            await _next(context);
        }
    }
    /* Включення класу SessionAuthMiddleware у ланцюг Middleware здійснюється
     * у Program.cs командою
     *     app.UseMiddleware<SessionAuthMiddleware>()
     * Традиційно Middleware забезпечують розширеннями (extensions) для
     * вживання UseXxxxx формалізму (Xxxxx - назва Middleware)
     *     app.UseSessionAuth();
     */
    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
