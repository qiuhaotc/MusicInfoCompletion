using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicInfoCompletion.Common;
using MusicInfoCompletion.Data;
using MusicInfoCompletion.Index;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace MusicInfoCompletion.Server
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
            services.AddVersionedApiExplorer(
                   options =>
                   {
                       options.GroupNameFormat = "'v'VVV";
                       options.SubstituteApiVersionInUrl = true;
                   });

            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });

            // Register the Swagger services
            services.AddSwaggerDocument(config =>
            {
                config.Title = "Music Server API";

                config.DocumentProcessors.Add(new SecurityDefinitionAppender(
                    "basic",
                    new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.Basic,
                        Name = "Authorization",
                        In = OpenApiSecurityApiKeyLocation.Header
                    }));

                config.OperationProcessors.Add(new OperationSecurityScopeProcessor("basic"));
            });

            services.AddAuthentication("SimpleAuthentication").AddScheme<AuthenticationSchemeOptions, SimpleAuthenticationHandler>("SimpleAuthentication", null);

            services.AddControllersWithViews();

            var musicConfiguration = new MusicConfiguration();
            Configuration.GetSection("MusicConfiguration").Bind(musicConfiguration);

            services.AddSingleton(musicConfiguration);

            services.AddDbContextPool<MusicDbContext>(option =>
            {
                option.UseMySql(Configuration.GetConnectionString("MusicConnection"));
            });

            services.AddSingleton<IndexMaintainer>();

            var authentication = new AuthenticationInfo();
            Configuration.Bind("AuthenticationInfo", authentication);
            services.AddSingleton(authentication);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifeTime, IndexMaintainer maintainer, MusicConfiguration musicConfiguration, ILogger<Startup> logger)
        {
            Logger = logger;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Register the Swagger generator and the Swagger UI middlewares
            app.UseOpenApi();
            app.UseSwaggerUi3();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            // on exit
            // lifeTime.ApplicationStopping.Register(OnShutdown);
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            Maintainer = maintainer;

            if (!Maintainer.IndexExist())
            {
                Task.Run(async () =>
                {
                    await Maintainer.InitIndex(CancellationTokenSource.Token);
                });
            }

            Task.Run(async () =>
            {
                while (!CancellationTokenSource.Token.IsCancellationRequested)
                {
                    Logger.LogInformation("Auto SaveResults start, will run after {AutoSaveIndexSeconds} seconds", musicConfiguration.AutoSaveIndexSeconds);
                    await Task.Delay(musicConfiguration.AutoSaveIndexSeconds * 1000, CancellationTokenSource.Token);
                    Logger.LogInformation("Auto SaveResults start");
                    await Maintainer.SaveResults(CancellationTokenSource.Token);
                    Logger.LogInformation("Auto SaveResults finished");
                }
            }, CancellationTokenSource.Token);
        }

        void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Logger.LogInformation("Shutdown Dispose start");
            CancellationTokenSource.Cancel();
            Maintainer?.Dispose();
            CancellationTokenSource.Dispose();
            Logger.LogInformation("Shutdown Dispose finished");
            NLog.LogManager.Shutdown();
        }

        CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
        IndexMaintainer Maintainer { get; set; }
        ILogger<Startup> Logger { get; set; }
    }
}
