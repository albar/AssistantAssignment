using Albar.AssistantAssignment.WebApp.Hubs;
using Albar.AssistantAssignment.WebApp.Services;
using Albar.AssistantAssignment.WebApp.Services.DatabaseTask;
using Albar.AssistantAssignment.WebApp.Services.ParallelGeneticAlgorithm;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Albar.AssistantAssignment.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private readonly string AllowSpecificOrigins = "_allowedSpecificOrigins";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
//            services.Configure<CookiePolicyOptions>(options =>
//            {
//                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
//                options.CheckConsentNeeded = context => true;
//                options.MinimumSameSitePolicy = SameSiteMode.None;
//            });

            services.AddCors(options =>
            {
                options.AddPolicy(AllowSpecificOrigins,
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:8080")
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    });
            });
            
            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR().AddNewtonsoftJsonProtocol();

            services.AddDbContext<AssignmentDatabase>(option => option.UseSqlite("Data Source=assignment.db"));

            services.AddHostedService<QueuedDatabaseBackgroundTask>();
            services.AddSingleton<IDatabaseBackgroundTaskQueue, DatabaseBackgroundTaskQueue>();

            services.AddHostedService<QueuedParallelGeneticAlgorithmBackgroundTask>();
            services.AddSingleton<IGeneticAlgorithmBackgroundTaskQueue, GeneticAlgorithmBackgroundTaskQueue>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors(AllowSpecificOrigins);

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseEndpoints(endpoint =>
            {
                endpoint.MapControllers();
                endpoint.MapHub<GeneticAlgorithmNotificationHub>("/notification");
            });
        }
    }
}