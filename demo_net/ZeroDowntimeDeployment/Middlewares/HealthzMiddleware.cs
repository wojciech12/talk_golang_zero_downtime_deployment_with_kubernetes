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
            var path = context.Request.Path.Value.ToLowerInvariant();
            switch (path)
            {
                case "/healthz":
                    context.Response.StatusCode = _healthService.IsHealth()
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status500InternalServerError;
                    break;

                case "/dohealthz":
                    _healthService.SetHealth(true);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    break;

                case "/dounhealthz":
                    _healthService.SetHealth(false);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    break;

                default:
                    await _next(context);                
                    break;
            }
        }
    }
}
