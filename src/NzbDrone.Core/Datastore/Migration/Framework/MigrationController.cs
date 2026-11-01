using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Initialization;
using FluentMigrator.Runner.Processors;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Extensions.Logging;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    public interface IMigrationController
    {
        void Migrate(string connectionString, MigrationContext migrationContext, DatabaseType databaseType);
    }

    public class MigrationController : IMigrationController
    {
        private readonly Logger _logger;

        public MigrationController(Logger logger)
        {
            _logger = logger;
        }

        public void Migrate(string connectionString, MigrationContext migrationContext, DatabaseType databaseType)
        {
            var sw = Stopwatch.StartNew();

            _logger.Info("*** Migrating {0} ***", connectionString);

            var db = databaseType switch
            {
                DatabaseType.SQLite => ProcessorIdConstants.SQLite,
                DatabaseType.PostgreSQL => ProcessorIdConstants.PostgreSQL,
                _ => throw new NotImplementedException($"Unknown database type: {databaseType}")
            };

            var serviceProvider = new ServiceCollection()
                .AddLogging(b => b.AddNLog())
                .AddFluentMigratorCore()
                .Configure<RunnerOptions>(cfg => cfg.IncludeUntaggedMaintenances = true)
                .ConfigureRunner(builder => builder
                    .AddPostgres15_0()
                    .AddNzbDroneSQLite()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(Assembly.GetExecutingAssembly()).For.All())
                .Configure<TypeFilterOptions>(opt => opt.Namespace = "NzbDrone.Core.Datastore.Migration")
                .Configure<ProcessorOptions>(opt =>
                {
                    opt.PreviewOnly = false;
                    opt.Timeout = TimeSpan.FromMinutes(5);
                })
                .Configure<SelectingProcessorAccessorOptions>(cfg =>
                {
                    cfg.ProcessorId = db;
                })
                .Configure<SelectingGeneratorAccessorOptions>(cfg =>
                {
                    cfg.GeneratorId = db;
                })
                .BuildServiceProvider();

            using (var scope = serviceProvider.CreateScope())
            {
                ReleaseUpstreamVersionsUsedByFork(scope.ServiceProvider);

                var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

                MigrationContext.Current = migrationContext;

                if (migrationContext.DesiredVersion.HasValue)
                {
                    runner.MigrateUp(migrationContext.DesiredVersion.Value);
                }
                else
                {
                    runner.MigrateUp();
                }

                MigrationContext.Current = null;
            }

            sw.Stop();

            _logger.Debug("Took: {0}", sw.Elapsed);
        }

        // Has to happen before the runner loads the applied versions, otherwise an upstream migration
        // reusing one of these versions is skipped for this run.
        private void ReleaseUpstreamVersionsUsedByFork(IServiceProvider serviceProvider)
        {
            var processor = serviceProvider.GetRequiredService<IProcessorAccessor>().Processor;
            var versionTable = serviceProvider.GetRequiredService<IVersionTableMetaData>();

            if (!processor.TableExists(versionTable.SchemaName, versionTable.TableName))
            {
                return;
            }

            var conditions = ForkMigrationVersions.ReleasedUpstreamVersions
                .Select(v => $"(\"{versionTable.ColumnName}\" = {v.Key} AND \"{versionTable.DescriptionColumnName}\" = '{v.Value}')");

            processor.Execute($"DELETE FROM \"{versionTable.TableName}\" WHERE {string.Join(" OR ", conditions)}");
        }
    }
}
