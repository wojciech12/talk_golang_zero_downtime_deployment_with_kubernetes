using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ZeroDowntimeDeployment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AssemblyLoadContext.Default.Unloading += OnUnloading;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            CreateWebHostBuilder(args).Build().Run();
        }

        private static void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private static void OnUnloading(AssemblyLoadContext obj)
        {
            throw new NotImplementedException();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
