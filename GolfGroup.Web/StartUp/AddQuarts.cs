using System;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GolfGroup.Api.StartUp
{
  public static class AddQuarts
  {
    public static IServiceCollection ConfigureQuarts(this IServiceCollection services)
    {
      // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
      // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
      services.AddQuartz(options =>
      {
        options.UseMicrosoftDependencyInjectionJobFactory();
        options.UseSimpleTypeLoader();
        options.UseInMemoryStore();
      });

      // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
      services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);


      return services;
    }
  }
}
