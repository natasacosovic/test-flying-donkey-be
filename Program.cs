using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace task_test_flying_donkey
{
  public class Program
  {
    public static IConfiguration Configuration { get; set; }

    public static void Main(string[] args)
    {
      var builder = new ConfigurationBuilder()
         .AddJsonFile("appsettings.json");
      Configuration = builder.Build();
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
              webBuilder.UseStartup<Startup>()
              .UseUrls(Configuration["URL"]);
            });
  }
}
