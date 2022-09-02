using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;

namespace eShop.BuildingBlocks.Logging.CommonLogging;

public class SeriLogger
{
    public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
        (context, logConfiguration) =>
        {
            var seqServerUrl = context.Configuration["SeqServerUrl"];
            var logstashUrl = context.Configuration["LogstashServerUrl"];

            logConfiguration.
            MinimumLevel.Verbose()
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
            .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(new RenderedCompactJsonFormatter(), "log.ndjson", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Seq(string.IsNullOrEmpty(seqServerUrl) ? "http://seq" : seqServerUrl)
            .WriteTo.Http(string.IsNullOrEmpty(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
            .ReadFrom.Configuration(context.Configuration);
        };
}
