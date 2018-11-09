using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZeroDowntimeDeployment.Services;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class LivenessProbeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHealthService _healthService;

        public LivenessProbeMiddleware(
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
                case "/probe":
                // Support for Zero downtime deployment with kubernetes scenarios
                case "/healthz":
                    var health = _healthService.IsHealth();
                    context.Response.StatusCode = health
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status500InternalServerError;
                    var body = health ? "OK" : "NOT OK";
                    using (var streamWriter = new StreamWriter(context.Response.Body))
                    {
                        await streamWriter.WriteAsync(body);
                    }
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
