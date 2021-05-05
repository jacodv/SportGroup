using System;
using AutoMapper;
using GolfGroup.Api.Data;
using GolfGroup.Api.Interfaces;
using GolfGroup.Api.Models;
using GolfGroup.Api.Settings;
using GolfGroup.Api.StartUp;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Serilog;

namespace GolfGroup.Api
{
  public class Startup
  {
    public Startup(IWebHostEnvironment env, IConfiguration configuration)
    {
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
      services.AddScoped(serviceProvider =>
      {
        databaseSettings = serviceProvider.GetRequiredService<IOptions<DatabaseSettings>>().Value;
        return new MongoClient(databaseSettings.ConnectionString).GetDatabase(databaseSettings.DatabaseName);
      });

      services.AddAutoMapper(typeof(Startup));

      services.AddScoped(typeof(IRepository<>), typeof(MongoRepository<>));

      services.ConfigureIdentity(Configuration);
      services.ConfigureQuarts();
      services.SetupOpenIddict(Configuration);

      services.AddControllers()
        .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

      // Register the worker responsible of seeding the database with the sample clients.
      // Note: in a real world application, this step should be part of a setup script.
      services.AddHostedService<Worker>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider)
    {
      SetupDatabase.Init(serviceProvider).Wait();

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
