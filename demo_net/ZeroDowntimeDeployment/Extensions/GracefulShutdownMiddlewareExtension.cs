using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ZeroDowntimeDeployment.Middlewares;

namespace ZeroDowntimeDeployment.Extensions
{
    public static class GracefulShutdownMiddlewareExtension
    {
        public static IApplicationBuilder UseGracefulShutdown(this IApplicationBuilder app)
        {
            var applicationLifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();
            app.UseMiddleware<GracefulShutdownMiddleware>(applicationLifetime);

            return app;
        }
    }
}
