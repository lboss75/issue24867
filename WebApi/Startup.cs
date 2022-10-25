using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApi
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<WebApi.DbModel>(options => options.UseInMemoryDatabase("InMemory"));

            services
                .AddControllers()
                .AddOData(opt =>
                    opt
                    .AddRouteComponents("odata", ODataModel.GetEdmModel())
                    .Select().Expand().Filter().OrderBy()
                );
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseEndpoints(enpoint =>
            {
                enpoint.MapControllers();
            });
        }
    }
}
