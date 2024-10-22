using Quartz;

namespace BeachApplication.Extensions;

public static class QuartzExtensions
{
    public static IServiceCollectionQuartzConfigurator AddScheduledJob<TJob>(this IServiceCollectionQuartzConfigurator configurator, string cronSchedule) where TJob : class, IJob
    {
        var jobName = typeof(TJob).Name;
        configurator.AddJob<TJob>(options => options.WithIdentity(jobName));

        configurator.AddTrigger(options =>
        {
            options.ForJob(jobName)
                .WithIdentity($"{jobName}-trigger")
                .WithCronSchedule(cronSchedule);
        });

        return configurator;
    }
}