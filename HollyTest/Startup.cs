using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HollyTest.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace HollyTest
{
    public class AddAuthorizeFiltersControllerConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action.Controller.ControllerName.Contains("Api"))
            {
                action.Filters.Add(new AuthorizeFilter("apipolicy"));
            }
            else
            {
                action.Filters.Add(new AuthorizeFilter("Private"));
            }
        }
    }

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
            services.AddDbContext<ApplicationDbContext>( options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")) );
            //services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddSingleton<WeatherForecastService>();

            // We should add this later!
            //services.AddCognitoIdentity();

            //services.AddControllersWithViews(options =>
            //{
            //    var policy = new AuthorizationPolicyBuilder()
            //        .RequireAuthenticatedUser()
            //        .Build();

            //    options.Filters.Add(new AuthorizeFilter(policy));
            //});

            //services.AddMvc( options =>
            //{
            //    options.Conventions.Add( new AddAuthorizeFiltersControllerConvention() );
            //});

            //services.AddMvc()
            //    .AddRazorPagesOptions(options =>
            //    {
            //        options.Conventions.AuthorizeFolder("/Test");
            //        options.Conventions.AllowAnonymousToPage("/");
            //        //options.Conventions.AllowAnonymousToPage("/");
            //    })
            //    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Private", policy => policy.RequireAuthenticatedUser());
                //options.AddPolicy("AtLeast21", policy =>
                //    policy.Requirements.Add(new MinimumAgeRequirement(21)));
            });

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("Private", policy => policy.RequireAuthenticatedUser() );
            //});

            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("Authentication:Cognito"));

            services.AddHttpContextAccessor();

            var serviceProvider = services.BuildServiceProvider();
            var authOptions = serviceProvider.GetService<IOptions<OpenIdConnectOptions>>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie( options =>
            {
                options.LoginPath = "/";
            })
            .AddOpenIdConnect(options =>
            {
                options.ResponseType = authOptions.Value.ResponseType;
                options.MetadataAddress = authOptions.Value.MetadataAddress;
                options.ClientId = authOptions.Value.ClientId;
                options.ClientSecret = authOptions.Value.ClientSecret;
                options.GetClaimsFromUserInfoEndpoint = authOptions.Value.GetClaimsFromUserInfoEndpoint;
                options.SaveTokens = authOptions.Value.SaveTokens;
                options.RequireHttpsMetadata = authOptions.Value.RequireHttpsMetadata;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = authOptions.Value.TokenValidationParameters.ValidateIssuer,
                    NameClaimType = authOptions.Value.TokenValidationParameters.NameClaimType
                };

                options.Events = new OpenIdConnectEvents
                {
                    OnRedirectToIdentityProviderForSignOut = context =>
                    {
                        var logoutUri = "hollytest.auth.ap-southeast-2.amazoncognito.com/logout";
                        var baseUri = "https://localhost:44380";

                        logoutUri += $"?client_id={authOptions.Value.ClientId}&logout_uri={baseUri}";
                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

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
