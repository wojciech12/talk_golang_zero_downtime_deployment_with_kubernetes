using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZeroDowntimeDeployment.Services;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class HealthzMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHealthService _healthService;

        public HealthzMiddleware(
            RequestDelegate next,
            IHealthService healthService)
        {
            _next = next;
            _healthService = healthService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            if (path.Value.ToLowerInvariant() == "/healthz")
            {
                // Determine application health
                context.Response.StatusCode = _healthService.IsHealth() 
                    ? StatusCodes.Status200OK
                    : StatusCodes.Status500InternalServerError;
            }
            else
            {
                // Call the next delegate/middleware in the pipeline
                await _next(context);                
            }
        }
    }
}
