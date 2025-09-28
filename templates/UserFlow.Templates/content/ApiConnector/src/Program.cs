using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectName;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<BasicAuthMiddleware>();
    })
    .ConfigureServices(_ => { })
    .ConfigureLogging(loggingBuilder => loggingBuilder.AddFilter("Microsoft", LogLevel.Information));

var app = builder.Build();
await app.RunAsync();
