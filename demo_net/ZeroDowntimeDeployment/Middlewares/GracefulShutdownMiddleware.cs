using System;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
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

        public GracefulShutdownMiddleware(RequestDelegate next, IApplicationLifetime applicationLifetime)
        {
            _next = next;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AssemblyLoadContext.Default.Unloading += OnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            applicationLifetime.ApplicationStopping.Register(OnStopping);
            applicationLifetime.ApplicationStopped.Register(OnStopped);
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
                --_concurrentRequests;
            }
        }

        public void OnShutdown()
        {
            lock (_shutdownLock)
            {
                _shutdown = true;
            }
            
            // Some wait for requests completion
            // how to wait with non blocking manner for requests completion?
            int concurentRequests = int.MaxValue;
            while (concurentRequests != 0)
            {
                lock (_concurrentRequestsLock)
                {
                    concurentRequests = _concurrentRequests;
                }
            }
        }

        private void OnStopped()
        {
            OnShutdown();
        }

        private void OnStopping()
        {
            OnShutdown();
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            OnShutdown();
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            OnShutdown();
        }

        private void OnUnloading(AssemblyLoadContext obj)
        {
            OnShutdown();
        }
    }
}
