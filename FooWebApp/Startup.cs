﻿using FooWebApp.Services;
using FooWebApp.Store;
using FooWebApp.Store.DocumentDb;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace FooWebApp
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

            string instrumentationKey = Configuration["ApplicationInsights:InstrumentationKey"];
            services.AddApplicationInsightsTelemetry(instrumentationKey);

            services.AddSingleton<IStudentStore, InMemoryStudentStore>();
            services.AddSingleton<IStudentService, StudentService>();

            services.AddOptions();
            services.Configure<AzureStorageSettings>(Configuration.GetSection("AzureStorageSettings"));

            services.Configure<DocumentDbSettings>(Configuration.GetSection(nameof(DocumentDbSettings)));
            services.AddSingleton<IDocumentClient>(sp =>
            {
                var settings = GetSettings<DocumentDbSettings>();
                return new DocumentClient(new Uri(settings.EndpointUrl), settings.PrimaryKey,
                    new ConnectionPolicy
                    {
                        ConnectionMode = ConnectionMode.Direct,
                        ConnectionProtocol = Protocol.Tcp,
                        MaxConnectionLimit = settings.MaxConnectionLimit
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseMiddleware<ExceptionMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private T GetSettings<T>() where T : new()
        {
            var config = Configuration.GetSection(typeof(T).Name);
            T settings = new T();
            config.Bind(settings);
            return settings;
        }
    }
}
