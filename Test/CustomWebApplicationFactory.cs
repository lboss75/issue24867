using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Test
{
    public class CustomWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.Test.json");
            builder.ConfigureServices(services =>
            {
                foreach (var exist in services.Where(descriptor => descriptor.ServiceType == typeof(WebApi.DbModel)).ToList())
                {
                    services.Remove(exist);
                }

                services.AddDbContext<WebApi.DbModel>(options => options.UseInMemoryDatabase("InMemory"));
            });
            builder.ConfigureAppConfiguration((context, conf) =>
            {
                conf.AddJsonFile(configPath, true);
            });
        }
    }
}
