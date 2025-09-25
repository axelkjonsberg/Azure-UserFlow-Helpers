using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker;

var builder = Host.CreateDefaultBuilder(args)
  .ConfigureFunctionsWorkerDefaults(worker =>
  {
      worker.UseMiddleware<BasicAuthMiddleware>();
  })
  .ConfigureServices(services =>
  {
  })
  .ConfigureLogging(lb => lb.AddFilter("Microsoft", LogLevel.Information));

var app = builder.Build();
await app.RunAsync();
