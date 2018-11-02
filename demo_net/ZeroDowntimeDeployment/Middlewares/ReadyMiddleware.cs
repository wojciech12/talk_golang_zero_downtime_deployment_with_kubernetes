using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ZeroDowntimeDeployment.Services;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class ReadyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IReadyService _readyService;

        public ReadyMiddleware(
            RequestDelegate next,
            IReadyService readyService)
        {
            _next = next;
            _readyService = readyService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value.ToLowerInvariant();
            switch (path)
            {
                case "/ready":
                    context.Response.StatusCode = _readyService.IsReady()
                        ? StatusCodes.Status200OK
                        : StatusCodes.Status500InternalServerError;
                    break;

                case "/doready":
                    _readyService.SetReady(true);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    break;

                case "/donotready":
                    _readyService.SetReady(false);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    break;

                default:
                    await _next(context);
                    break;
            }
        }
    }
}
