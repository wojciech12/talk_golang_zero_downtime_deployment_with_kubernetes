using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ZeroDowntimeDeployment.Services;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class ReadyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IReadyService _readyService;
        private readonly ILogger<ReadyMiddleware> _logger;
        private readonly string _hostname;

        public ReadyMiddleware(
            RequestDelegate next,
            IReadyService readyService,
            ILogger<ReadyMiddleware> logger)
        {
            _next = next;
            _readyService = readyService;
            _logger = logger;
            _hostname = Environment.GetEnvironmentVariable("HOSTNAME");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value.ToLowerInvariant();
            switch (path)
            {
                case "/ready":
                    var ready = _readyService.IsReady();
                    context.Response.StatusCode = ready
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status500InternalServerError;
                    var body = ready ? $"OK. Node: {_hostname}" : $"Not Ready. Node: {_hostname}";
                    using (var streamWriter = new StreamWriter(context.Response.Body))
                    {
                        await streamWriter.WriteAsync(body);
                    }
                    break;

                case "/doready":
                    _readyService.SetReady(true);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    _logger.LogInformation("Switched ready!");
                    break;

                case "/donotready":
                    _readyService.SetReady(false);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    _logger.LogInformation("Switched unready!");
                    break;

                default:
                    await _next(context);
                    break;
            }
        }
    }
}
