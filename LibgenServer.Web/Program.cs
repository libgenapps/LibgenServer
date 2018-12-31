using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace LibgenServer.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args).
                ConfigureLogging(logging => logging.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement", LogLevel.Warning)).
                UseStartup<Startup>();
    }
}
