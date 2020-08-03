namespace BasicSalesSystem.Web
{
    using System;
    using System.Text;
    using BasicSalesSystem.Domain.Entities;
    using BasicSalesSystem.Web.Custom;
    using BasicSalesSystem.Web.Dependencies.EmailService;
    using BasicSalesSystem.Web.Dependencies.MongoDbService;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.AspNetCore.SpaServices;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Newtonsoft.Json.Serialization;
    using VueCliMiddleware;

    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // this method gets called by the runtime. use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddRazorPages();

            services
                .AddRouting(options =>
                {
                    options.LowercaseUrls = true;
                })
                .AddControllersWithViews(options =>
                {
                    var authorizationPolicy = new AuthorizationPolicyBuilder(new[] { JwtBearerDefaults.AuthenticationScheme })
                        .RequireAuthenticatedUser()
                        .Build();

                    options.Filters.Add(new AuthorizeFilter(authorizationPolicy));
                    options.Filters.Add(new CustomModelStateFilter());
                })
                .AddFluentValidation(options =>
                {
                    options.RegisterValidatorsFromAssemblyContaining<Startup>();
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            #region SERVICES
            services.AddScoped<EmailService>();
            services.AddScoped<MongoDbService>();
            #endregion

            #region IDENTITY
            services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.User.RequireUniqueEmail = false;
                    options.User.AllowedUserNameCharacters = null;

                    options.Password.RequireDigit = false;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = false;

                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddRoles<ApplicationRole>()
                //.AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            #endregion

            #region AUTHENTICATION
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    var secret = _configuration.GetSection("JwtSettings:Secret").Value;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidAudience = _configuration.GetSection("JwtSettings:ValidAudience").Value,
                        ValidateAudience = _configuration.GetSection("JwtSettings:ValidateAudience").Get<bool>(),
                        ValidIssuer = _configuration.GetSection("JwtSettings:ValidIssuer").Value,
                        ValidateIssuer = _configuration.GetSection("JwtSettings:ValidateIssuer").Get<bool>(),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret))
                    };
                });
            #endregion

            #region SWAGGER CONFIGURATION
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });
                options.AddSecurityDefinition("JwtAuth", new OpenApiSecurityScheme()
                {
                    Name = "Bearer",
                    BearerFormat = "JWT",
                    Scheme = "bearer",
                    Description = "Specify the authorization token.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                    {
                        { new OpenApiSecurityScheme() { Reference = new OpenApiReference(){ Id = "JwtAuth", Type = ReferenceType.SecurityScheme }}, new string[] { } }
                    });
            });
            #endregion

            // in production, the vue files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
        }

        // this method gets called by the runtime. use this method to configure the http request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsProduction())
            {
                app.UseHsts();
                app.UseSpaStaticFiles();
            }

            app.UseCors(options =>
            {
                options.AllowAnyOrigin();
                options.AllowAnyMethod();
                options.AllowAnyHeader();
            });

            app.UseStatusCodePages();
            app.UseExceptionHandlerMiddleware();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSwagger();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");

                if (env.IsDevelopment())
                {
                    endpoints.MapToVueCliProxy(
                        "{*path}",
                        new SpaOptions { SourcePath = "ClientApp" },
                        npmScript: "serve",
                        regex: "Compiled successfully");
                }
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "basicsalessystem.api");
            });
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment())
                {
                    spa.Options.StartupTimeout = TimeSpan.FromMinutes(5);
                }
            });
        }
    }
}
