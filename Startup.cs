using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using RestSharp;

using gardenit_razor.Data;
using gardenit_razor.Api;
using gardenit_razor.Data.Storage;

namespace gardenit_razor
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
            services.AddRazorPages();

            // Basic DI
            services.AddScoped<PlantService>();
            services.AddScoped<IRestClient, RestSharp.RestClient>();

            // Identity and EF
            var identityDbConnString = Configuration.GetConnectionString("IdentityDb")?? 
                Environment.GetEnvironmentVariable("IdentityDb");
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(identityDbConnString));
            services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            // Api injection
            services.AddScoped<IApi, PlantApi>();
            var apiUrl = Configuration["ApiOptions:Url"] ?? 
                Environment.GetEnvironmentVariable("apiUrl");
            var encryptionKey = Configuration["ApiOptions:EncryptionKey"] ?? 
                Environment.GetEnvironmentVariable("apiEncryptionKey");

            var apiOptions = new ApiOptions() {
                Url = apiUrl,
                EncryptionKey = encryptionKey
            };
            services.AddSingleton<IOptions<ApiOptions>>(x => Options.Create(apiOptions));

            services.AddScoped<IEncryptor, Encryptor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Apply migrations
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>())
                {
                    context.Database.Migrate();
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
