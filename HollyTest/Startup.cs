using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using HollyTest.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HollyTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            var authOptions = serviceProvider.GetService<IOptions<OpenIdConnectOptions>>();

            services.AddAuthentication( options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddOpenIdConnect(options => {
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.MetadataAddress = "https://cognito-idp.ap-southeast-2.amazonaws.com/ap-southeast-2_QQIXmkpHD/.well-known/openid-configuration";
                    options.ClientId = "238f6lc55s5titur3p1m2d7noa";
                    options.ClientSecret = "3rvkl2hc2pl8ho4p1hdftr3a41meq8cslabgrm3289480kjnf8f";
                    options.GetClaimsFromUserInfoEndpoint = true;
                    //options.SaveTokens = authOptions.Value.SaveTokens;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false
                    };
                });
            //services.Configure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
            //{
            //    options.SignInScheme = "";
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        // Instead of using the default validation (validating against a single issuer value, as we do in
            //        // line of business apps), we inject our own multitenant validation logic
            //        ValidateIssuer = false,

            //        // If the app is meant to be accessed by entire organizations, add your issuer validation logic here.
            //        //IssuerValidator = (issuer, securityToken, validationParameters) => {
            //        //    if (myIssuerValidationLogic(issuer)) return issuer;
            //        //}
            //    };

            //    options.Events = new OpenIdConnectEvents
            //    {
            //        OnTicketReceived = context =>
            //        {
            //            // If your authentication logic is based on users then add your logic here
            //            return Task.CompletedTask;
            //        },
            //        OnAuthenticationFailed = context =>
            //        {
            //            context.Response.Redirect("/Error");
            //            context.HandleResponse(); // Suppress the exception
            //            return Task.CompletedTask;
            //        },
            //        // If your application needs to authenticate single users, add your user validation below.
            //        //OnTokenValidated = context =>
            //        //{
            //        //    return myUserValidationLogic(context.Ticket.Principal);
            //        //}
            //    };
            //});

            services.AddControllersWithViews(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
            });

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();
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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
