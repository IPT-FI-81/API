using System;
using System.Net.Http;
using Bijector.Infrastructure.Discovery;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace Bijector.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddConsul(Configuration);
            services.AddConsulDiscover();
            var discover = services.BuildServiceProvider().GetService<IServiceDiscover>();
            var accountsUrl = discover.ResolveServicePath("Bijector Accounts");

            Action<JwtBearerOptions> jwtOptions = o =>
            {      
                //o.Authority = accountsUrl;          
                o.Authority = $"http://{accountsUrl}";
                o.RequireHttpsMetadata = false;
                o.Audience = "api.v1";
                o.BackchannelHttpHandler = new HttpClientHandler{ServerCertificateCustomValidationCallback = (f1, s, t, f2) => true};
            };

            Action<OAuth2IntrospectionOptions> oauthOptions = o =>
            {
                //o.Authority = accountsUrl;        
                o.Authority = $"http://{accountsUrl}";        
            };
            
            services.AddAuthentication().AddIdentityServerAuthentication("Accounts", jwtOptions, oauthOptions);

            services.AddOcelot().AddConsul();
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime lifetime)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }            

            app.UseHttpsRedirection();
            
            app.UseConsul(lifetime);

            app.UseRouting();

            app.UseAuthorization();

            app.UseAuthentication();          

            app.UseOcelot().Wait();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
