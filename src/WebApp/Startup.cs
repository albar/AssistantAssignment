using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using AssistantAssignment.Data.Database;
using AssistantAssignment.WebApp.Repositories;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmBuilderService;
using AssistantAssignment.WebApp.Services.GeneticAlgorithmRunnerService;

namespace AssistantAssignment.WebApp
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
            services.AddControllers();
            services.AddDbContext<DatabaseContext>(option =>
                option.UseSqlite("Data Source=assignment.db"));
            services.AddGeneticAlgorithmTaskRepository();
            services.AddGeneticAlgorithmBuilderService();
            services.AddGeneticAlgorithmRunnerService();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
