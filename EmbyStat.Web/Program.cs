using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CommandLine;
using EmbyStat.Common;
using EmbyStat.Common.Models.Settings;
using EmbyStat.Repositories.Interfaces;
using EmbyStat.Repositories.Migrations;
using FluentMigrator.Runner;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Fluent;
using NLog.Web;
using Rollbar;

namespace EmbyStat.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog(Path.Combine("Settings", "nlog.config")).GetCurrentClassLogger();

            try
            {
                var result = Parser.Default.ParseArguments<StartupOptions>(args);
                StartupOptions options = null;
                result.WithParsed(opts => options = opts);

                var listeningUrl = $"http://*:{options.Port}";

                logger.Log(NLog.LogLevel.Info, $"{Constants.LogPrefix.System}\tBooting up server on port {options.Port}");
                var configArgs = new Dictionary<string, string> { { "Port", options.Port.ToString() } };
                var config = BuildConfigurationRoot(configArgs);
                var host = BuildWebHost(args, listeningUrl, config);

                SetupDatabase(host);

                host.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Log(NLog.LogLevel.Fatal, ex, $"{Constants.LogPrefix.System}\tServer terminated unexpectedly");
            }
            finally
            {
                Console.WriteLine($"{Constants.LogPrefix.System}\tServer shutdown");
                logger.Log(NLog.LogLevel.Info, $"{Constants.LogPrefix.System}\tServer shutdown");
                NLog.LogManager.Flush();
                NLog.LogManager.Shutdown();
            }
        }

        public static IWebHost BuildWebHost(string[] args, string listeningUrl, IConfigurationRoot config) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseIISIntegration()
                .UseUrls(listeningUrl)
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .UseNLog()
                .Build();

        public static IConfigurationRoot BuildConfigurationRoot(Dictionary<string, string> configArgs) =>
            new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .AddInMemoryCollection(configArgs)
                .Build();

        private static void SetupDatabase(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    UpdateDatabase();
                    var databaseInitializer = services.GetRequiredService<IDatabaseInitializer>();
                    databaseInitializer.SeedAsync().Wait();
                }
                catch (Exception ex)
                {
                    Log.Fatal($"{Constants.LogPrefix.System}\tDatabase seed or update failed");
                    Log.Fatal($"{Constants.LogPrefix.System}\t{ex.Message}\n{ex.StackTrace}");
                    throw;
                }
            }
        }

        private static void UpdateDatabase()
        {
            var serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSQLite()
                    .WithGlobalConnectionString("Data Source=data.db")
                    .ScanIn(typeof(InitMigration).Assembly).For.Migrations())
                .BuildServiceProvider(false);

            var runner = serviceProvider.GetRequiredService<IMigrationRunner>();
            runner.MigrateUp();
        }
    }
}

