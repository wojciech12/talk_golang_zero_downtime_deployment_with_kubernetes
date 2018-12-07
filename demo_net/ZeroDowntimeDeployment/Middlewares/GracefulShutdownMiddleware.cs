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

        private static int _shutdown;
        private const int ShutdownUnlocked = 0;
        private const int ShutdownSet = 1;

        private readonly ManualResetEventSlim _unloadingEvent = new ManualResetEventSlim();

        public GracefulShutdownMiddleware(
            RequestDelegate next,
            ILogger<GracefulShutdownMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _shutdown = ShutdownUnlocked;

            AssemblyLoadContext.Default.Unloading += OnUnloading;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestId = Guid.NewGuid();

            if (_shutdown == ShutdownSet)
            {
                _logger.LogInformation($"GracefulShutdownMiddleware InvokeAsync {requestId} on shutdown");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }

            Interlocked.Increment(ref _concurrentRequests);

            _logger.LogInformation($"GracefulShutdownMiddleware starting invoking next {requestId}");
            await _next(context);
            _logger.LogInformation($"GracefulShutdownMiddleware finished invoking next {requestId}");

            if (Interlocked.Decrement(ref _concurrentRequests) == 0 && _shutdown == ShutdownSet)
            {
                _unloadingEvent.Set();
            }
        }

        private void OnUnloading(AssemblyLoadContext obj)
        {
            _logger.LogInformation("GracefulShutdownMiddleware OnUnloading");
            _logger.LogInformation("Setting shutdown lock");
            Interlocked.Exchange(ref _shutdown, ShutdownSet);

            if (_concurrentRequests > 0)
            {
                _logger.LogInformation("Waiting for ongoing requests completion.");
                _unloadingEvent.Wait();
            }

            _logger.LogInformation("Last requests were processed, shutting down. In 5 sec.");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
