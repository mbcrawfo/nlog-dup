using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace nlog_dup
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Logger log = null;

            try
            {
                Console.WriteLine("Configuring web host");
                var host = WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .ConfigureLogging(
                        logging =>
                        {
                            logging.ClearProviders();
                            logging.SetMinimumLevel(LogLevel.Trace);
                        }
                    )
                    .UseNLog()
                    .Build();

                LogManager.Configuration = new NLogLoggingConfiguration(
                    host.Services.GetRequiredService<IConfiguration>().GetSection("NLog")
                );
                log = LogManager.GetCurrentClassLogger();
                log.Info("Web host and logging configured");

                log.Info("Starting web host");
                host.Run();
            }
            catch (Exception ex)
            {
                // something choked while setting up the config
                if (log is null)
                {
                    Console.Error.WriteLine("Unhandled exception in application: " + ex.Message);
                    Console.Error.WriteLine("Error occurred at " + ex.StackTrace);
                }
                else
                {
                    log.Fatal(ex, "Unhandled exception in application");
                }

                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}
