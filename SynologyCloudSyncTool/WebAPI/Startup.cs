using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using WebAPI.Actors;
using WebAPI.Data;
using WebAPI.IO;
using IApplicationLifetime = Microsoft.Extensions.Hosting.IApplicationLifetime;

namespace WebAPI
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
            services.AddSingleton(_ => ActorSystem.Create("SynologyFileManager"));
            services.AddSingleton<AzureStorageService>();
            
            services.AddSingleton<DecryptActorProvider>(provider =>
            {
                var actorSystem = provider.GetRequiredService<ActorSystem>();
                var storageService = provider.GetRequiredService<AzureStorageService>();
                var booksManagerActor = actorSystem.ActorOf(Props.Create(() => new AzureDownloadActor(storageService)));
                return () => booksManagerActor;
            });
            
            services.AddSingleton<AzureDownloadActorProvider>(provider =>
            {
                var actorSystem = provider.GetRequiredService<ActorSystem>();
                var storageService = provider.GetRequiredService<AzureStorageService>();
                var booksManagerActor = actorSystem.ActorOf(Props.Create(() => new AzureDownloadActor(storageService)));
                return () => booksManagerActor;
            });
            
            services.AddSingleton<ProjectActorProvider>(provider =>
            {
                var actorSystem = provider.GetRequiredService<ActorSystem>();
                var downloadProvider = provider.GetRequiredService<AzureDownloadActorProvider>();
                var projectActor = actorSystem.ActorOf(Props.Create(() => new ProjectActor(downloadProvider)));
                return () => projectActor;
            });

            services.AddControllers();
            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPI", Version = "v1" }); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPI v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
            
            lifetime.ApplicationStarted.Register(() =>
            {
                app.ApplicationServices.GetService<ActorSystem>(); // start Akka.NET
            });
            lifetime.ApplicationStopping.Register(() => { app.ApplicationServices.GetService<ActorSystem>()?.Terminate()?.Wait(); });
        }
    }
}