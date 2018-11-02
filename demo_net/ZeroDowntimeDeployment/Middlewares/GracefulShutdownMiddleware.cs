using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ZeroDowntimeDeployment.Middlewares
{
    public class GracefulShutdownMiddleware
    {
        private readonly RequestDelegate _next;

        private static int _concurrentRequests;
        private readonly object _concurrentRequestsLock = new object();

        private static bool _shutdown;
        private readonly object _shutdownLock = new object();

        private readonly ManualResetEventSlim _unloadingEvent = new ManualResetEventSlim();

        public GracefulShutdownMiddleware(RequestDelegate next)
        {
            _next = next;
            
            AssemblyLoadContext.Default.Unloading += OnUnloading;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            lock(_shutdownLock)
            {
                if (_shutdown)
                {
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    return;
                }
            }

            lock (_concurrentRequestsLock)
            {
                ++_concurrentRequests;
            }

            await _next(context);

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
            lock (_shutdownLock)
            {
                _shutdown = true;
            }

            lock (_concurrentRequestsLock)
            {
                if (_concurrentRequests == 0)
                    return;
            }
            _unloadingEvent.Wait();
        }
    }
}
