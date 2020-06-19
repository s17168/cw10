using Cw3WebApplication.DAL;
using Cw3WebApplication.Middleware;
using Cw3WebApplication.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Wyklad5.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Cw3WebApplication.NewFolder;
using Microsoft.EntityFrameworkCore;

namespace Cw3WebApplication
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
            services.AddScoped<IDbService, MsqlDbService>();
            services.AddScoped<IStudentsDbService, SqlServerStudentDbService>();
            services.AddDbContext<apbdContext>(options =>
            options.UseSqlServer("Data Source=DESKTOP-H0B9S2Q\\SQLEXPRESS;Initial Catalog=apbd;Integrated Security=True"));

            // Uwierzytelnianie za pomoca JWT token
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        // walidacja
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidIssuer = "Gakko",
                            ValidAudience = "Students",
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"])) //SecretKey jest odczytywane z secrets.json
                        };
                    });

            //services.AddScoped<>
            services.AddScoped<LoggingService>();
            services.AddControllers();

            // 1. Add documentation
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "Students API", Version = "v1" });
            });

            // Add this for basic auth with BasicAuthHandler
            //services.AddAuthentication("AuthenticationBasic")
            //      .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("AuthenticationBasic", null);


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, 
            IStudentsDbService service)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionMiddleware>();

            // 2. Add documentation - add middleware
            app.UseSwagger();
            app.UseSwaggerUI( config => {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "Students App API");
            });

            // Add LoggingMiddleware
            app.UseMiddleware<LoggingMiddleware>();

            app.UseRouting();

            // disable middleware which checks for index in header
            /*app.Use(async (context, next) =>
            {
                if (!context.Request.Headers.ContainsKey("Index")) 
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Musisz podac numer indeksu");
                    return;
                }
                string index = context.Request.Headers["Index"].ToString();
                var student = service.GetStudent(index);
                if (student == null) 
                {
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    await context.Response.WriteAsync("Student o podanym numerze indeksu nie istnieje");
                    return;
                }

                await next(); // idziemy do kolejnego middleware
            });
            */

            //app.UseAuthentication(); add for basic auth
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
