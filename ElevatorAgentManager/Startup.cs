using Elevator.Agent.Manager.Api.AgentCommunication;
using Elevator.Agent.Manager.Api.Hosting;
using Elevator.Agent.Manager.Api.Hosting.StartAgents;
using Elevator.Agent.Manager.Api.Hosting.Tasks;
using Elevator.Agent.Manager.Api.Models;
using Elevator.Agent.Manager.Api.Repositories;
using Elevator.Agent.Manager.Queue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Repositories.Database;
using Repositories.Repositories;

namespace Elevator.Agent.Manager.Api
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
                }); ;

            services.Configure<StartAgentServiceConfig>(configuration.GetSection("StartAgentService"));

            services.AddLogging();

            services.AddHostedService<StartAgentService>();
            services.AddHostedService<TasksService>();

            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<DatabaseContext>();

            services.AddScoped<ProjectRepository>();
            services.AddScoped<BuildConfigRepository>();
            services.AddScoped<BuildStepRepository>();
            services.AddScoped<BuildRepository>();
            services.AddScoped<UserRepository>();

            services.AddScoped<BuildTasksService>();

            services.AddSingleton<PriorityQueue<BuildTask>>();

            services.AddSingleton<AgentsService>();

            services.AddSingleton<ICurrentTasksRepository, CurrentTasksInMemoryRepository>();

            services.AddCors(c =>
            {
                c.AddPolicy("AllowOrigin",
                    options => options.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ElevatorAgentManager", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ElevatorAgentManager v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
