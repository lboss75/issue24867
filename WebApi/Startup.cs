using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Text.Json.Serialization;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = this.Configuration.GetSection("APICFG_CONNECTIONSTRING").Value;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString = Configuration.GetConnectionString("DefaultConnection");
            }
            
            //services.AddDbContext<Model.DbModel>(options => options.UseNpgsql(connectionString));

            services.AddCors();

            services
                .AddControllers()
                .AddOData(opt =>
                    opt
                    .AddRouteComponents("odata", ODataModel.GetEdmModel())
                    .Select().Expand().Filter().OrderBy()
                );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseCors(options =>
                    options.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseEndpoints(enpoint =>
            {
                enpoint.MapControllers();
            });
        }
    }
}
