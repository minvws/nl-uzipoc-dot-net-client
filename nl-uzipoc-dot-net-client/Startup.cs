using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace nl_uzipoc_dot_net_client
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
            var oidcConfiguration = Configuration.GetSection("Oidc").Get<OidcConfiguration>();

            services.AddRazorPages(options =>
            {
                options.Conventions.AllowAnonymousToPage("/Index");
            });
            services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "cookie";
                options.DefaultSignInScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("cookie")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.BackchannelHttpHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                        {
                            return true;
                        }
                    };
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Authority = oidcConfiguration.Authority ;
                    options.ClientId = oidcConfiguration.ClientId;
                    options.ResponseType = "code";
                    options.ResponseMode = "form_post";
                    options.UsePkce = true;
                    options.GetClaimsFromUserInfoEndpoint = false;
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenResponseReceived = context => UserInfoTool.FetchUserInfoOnTokenResponseReceived(
                            context, oidcConfiguration.ClientKeyPath, oidcConfiguration.ClientCertPath)                       
                    };
                    
                });
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
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages()
                .RequireAuthorization();
                
            });
        }
    }
}

