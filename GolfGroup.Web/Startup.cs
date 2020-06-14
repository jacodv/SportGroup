using System;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using GolfGroup.Api.Data;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using GolfGroup.Api.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GolfGroup.Api
{
  public class Startup
  {
    private readonly IWebHostEnvironment _env;

    public Startup(IWebHostEnvironment env, IConfiguration configuration)
    {
      _env = env;
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<DatabaseSettings>(Configuration.GetSection(nameof(DatabaseSettings)));

      IDatabaseSettings databaseSettings=new DatabaseSettings();
      services.AddSingleton(serviceProvider =>
        {
          databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
          return databaseSettings;
        });
      services.AddAutoMapper(typeof(Startup));

      var key = Encoding.ASCII.GetBytes(databaseSettings.Secret);
      services.AddAuthentication(x =>
        {
          x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
          x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(x =>
        {
          x.Events = new JwtBearerEvents
          {
            OnTokenValidated = context =>
            {
              var users = context.HttpContext.RequestServices.GetRequiredService<IRepository<User>>();
              var userName = context.Principal.Identity.Name;
              var user = users.FindOne(_=>_.Email==userName);
              if (user == null)
              {
                // return unauthorized if user no longer exists
                context.Fail("Unauthorized");
              }
              return Task.CompletedTask;
            }
          };
          x.RequireHttpsMetadata = false;
          x.SaveToken = true;
          x.TokenValidationParameters = new TokenValidationParameters
          {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
          };
        });

      services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

      services.AddControllers()
        .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
      if (env.IsDevelopment())
      {
        app.UseDeveloperExceptionPage();
      }

      app.UseHttpsRedirection();

      app.UseRouting();

      app.UseAuthentication();
      app.UseAuthorization();

      app.UseEndpoints(endpoints =>
      {
        endpoints.MapControllers();
      });
    }
  }
}
