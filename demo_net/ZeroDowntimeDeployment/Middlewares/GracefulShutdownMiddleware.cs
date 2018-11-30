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

        private static bool _shutdown;
        private static readonly SemaphoreSlim ShutdownSemaphore = new SemaphoreSlim(1, 1);

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

            await ShutdownSemaphore.WaitAsync();
            if (_shutdown)
            {
                ShutdownSemaphore.Release();
                _logger.LogInformation($"GracefulShutdownMiddleware InvokeAsync {requestId} on shutdown");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                return;
            }
            ShutdownSemaphore.Release();

            Interlocked.Increment(ref _concurrentRequests);

            _logger.LogInformation($"GracefulShutdownMiddleware starting invoking next {requestId}");
            await _next(context);
            _logger.LogInformation($"GracefulShutdownMiddleware finished invoking next {requestId}");
            
            if (Interlocked.Decrement(ref _concurrentRequests) == 0 && _shutdown)
            {
                _unloadingEvent.Set();
            }
        }

        private void OnUnloading(AssemblyLoadContext obj)
        {
            _logger.LogInformation("GracefulShutdownMiddleware OnUnloading");

            _logger.LogInformation("Setting shutdown lock");

            ShutdownSemaphore.Wait();
            _shutdown = true;
            ShutdownSemaphore.Release();

            if (_concurrentRequests == 0)
            {
                _logger.LogInformation("No concurrent requests in progress, shutting down. In 5 sec.");
                Thread.Sleep(TimeSpan.FromSeconds(5));
                return;
            }

            _unloadingEvent.Wait();
            _logger.LogInformation("Last requests were processed, shutting down. In 5 sec.");
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }
}
