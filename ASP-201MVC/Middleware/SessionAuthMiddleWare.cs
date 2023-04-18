using ASP_201MVC.Data;
using ASP_201MVC.Data.Entity;
using System.Runtime.CompilerServices;

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
            if(userId is not null)
            {
                try
                {
                    User? authUser = 
                    dataContext.Users.Find(Guid.Parse(userId));
                    if(authUser is not null)
                    {
                        context.Items.Add("authUser", authUser);
                    }
                }
                catch(Exception ex)
                {
                    logger.LogWarning(ex, "SessionAuthMiddleware");
                }
            }
            await _next(context);
        }
    }
    public static class SessionAuthMiddlewareExtension
    {
        public static IApplicationBuilder UseSessionAuth( this IApplicationBuilder app)
        {
            return app.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}
