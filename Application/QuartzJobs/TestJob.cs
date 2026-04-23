using Application.Processing;
using Infrastructure.Db.App;
using Infrastructure.Db.App.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Application.QuartzJobs;

/// <summary>
///
/// </summary>
[DisallowConcurrentExecution]
public sealed class TestJob : IJob
{
    private const int MaxGenerationAttempts = 3;


    public TestJob()
    {
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;

    }
}
