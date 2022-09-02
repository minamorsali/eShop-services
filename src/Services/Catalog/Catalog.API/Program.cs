using Catalog.API.Infrastructure;
using eShop.BuildingBlocks.Logging.CommonLogging;
using Polly;
using Serilog;
using System.Data.SqlClient;

var configuration = GetConfiguration();


try
{
    //Log.Logger = CreateSerilogLogger(configuration);
    var host = CreateHostBuilder(configuration, args);

    Log.Information("Configuration web host ({ApplicationContext})...", Program.AppName);
    Log.Information("Applying migrations ({ApplicationContext})...", Program.AppName);
    //migrations
    using (var scope = host.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
        var env = scope.ServiceProvider.GetService<IWebHostEnvironment>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<CatalogContextSeed>>();
        //db.Database.Migrate();

        var retry = Policy.Handle<SqlException>().WaitAndRetry(new TimeSpan[] { TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(8) });
        retry.Execute(() => { new CatalogContextSeed().MigrateAndSeedAsync(context, env, logger).Wait(); });
    }

    Log.Information("starting web host ({ApplicationContext})...", Program.AppName);
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpactedly");
}
finally
{
    Log.CloseAndFlush();
}

IConfiguration GetConfiguration()
{
    var path = Directory.GetCurrentDirectory();

    var builder = new ConfigurationBuilder()
        .SetBasePath(path)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddEnvironmentVariables();

    return builder.Build();
}


IHost CreateHostBuilder(IConfiguration configuration, string[] args) =>
    Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>()
                  .UseContentRoot(Directory.GetCurrentDirectory())
                  .UseWebRoot("Pics")
                  .ConfigureAppConfiguration(x => x.AddConfiguration(configuration))
                  .CaptureStartupErrors(false);
    })
    .UseSerilog(SeriLogger.Configure)
    .Build();

//Serilog.ILogger CreateSerilogLogger(IConfiguration configuration)
//{
//    var seqServerUrl = configuration["SeqServerUrl"];
//    var logstashUrl = configuration["LogstashgUrl"];
//    return new LoggerConfiguration()
//          .MinimumLevel.Verbose()
//              .Enrich.WithProperty("Environment", "Development")
//              .Enrich.WithProperty("Application", Program.AppName)
//              .Enrich.FromLogContext()
//              .WriteTo.Console()
//              .WriteTo.File(new RenderedCompactJsonFormatter(), "log.ndjson", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose)
//              .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day,
//              rollOnFileSizeLimit: true, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
//              .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
//              .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl, null)
//              .ReadFrom.Configuration(configuration).CreateLogger();
//}


public partial class Program
{
    public static string? Namespace = typeof(Startup).Namespace;
    public static string? AppName = "Catalog.API";
}