using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ZeroDowntimeDeployment.Services;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class LivenessProbeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHealthService _healthService;
        private readonly ILogger<LivenessProbeMiddleware> _logger;
        private readonly string _hostname;

        public LivenessProbeMiddleware(
            RequestDelegate next,
            IHealthService healthService,
            ILogger<LivenessProbeMiddleware> logger)
        {
            _next = next;
            _healthService = healthService;
            _logger = logger;
            _hostname = Environment.GetEnvironmentVariable("HOSTNAME");
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
                    var body = health ? $"OK. Node: {_hostname}" : $"Not working. Node: {_hostname}";
                    using (var streamWriter = new StreamWriter(context.Response.Body))
                    {
                        await streamWriter.WriteAsync(body);
                    }
                    break;

                case "/dohealthz":
                    _healthService.SetHealth(true);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    _logger.LogInformation("Switched healthy!");
                    break;

                case "/dounhealthz":
                    _healthService.SetHealth(false);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    _logger.LogInformation("Switched unhealthy!");
                    break;

                default:
                    await _next(context);                
                    break;
            }
        }
    }
}
