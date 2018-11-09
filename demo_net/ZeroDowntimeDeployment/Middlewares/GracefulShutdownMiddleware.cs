using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class GracefulShutdownMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GracefulShutdownMiddleware> _logger;

        private static int _concurrentRequests;
        private readonly object _concurrentRequestsLock = new object();

        private static bool _shutdown;
        private readonly object _shutdownLock = new object();

        private readonly ManualResetEventSlim _unloadingEvent = new ManualResetEventSlim();

        public GracefulShutdownMiddleware(
            RequestDelegate next,
            ILogger<GracefulShutdownMiddleware> logger)
        {
            _next = next;
            _logger = logger;

            AssemblyLoadContext.Default.Unloading += OnUnloading;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid();
            lock(_shutdownLock)
            {
                if (_shutdown)
                {
                    _logger.LogInformation($"GracefulShutdownMiddleware InvokeAsync {requestId} on shutdown");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }
            }

            lock (_concurrentRequestsLock)
            {
                ++_concurrentRequests;
            }

            _logger.LogInformation($"GracefulShutdownMiddleware starting invoking next {requestId}");
            await _next(context);
            _logger.LogInformation($"GracefulShutdownMiddleware finished invoking next {requestId}");

            lock (_concurrentRequestsLock)
            {
                if (--_concurrentRequests == 0 && _shutdown)
                {
                    _unloadingEvent.Set();
                }
            }
        }

        private void OnUnloading(AssemblyLoadContext obj)
        {
            _logger.LogInformation("GracefulShutdownMiddleware OnUnloading");
            lock (_shutdownLock)
            {
                _logger.LogInformation("Setting shutdown lock");
                _shutdown = true;
            }

            lock (_concurrentRequestsLock)
            {
                if (_concurrentRequests == 0)
                {
                    _logger.LogInformation("No concurrent requests in progress, shutting down. In 5 sec.");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    return;
                }
            }
            _unloadingEvent.Wait();
            _logger.LogInformation("Last requests were processed, shutting down. In 5 sec.");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
