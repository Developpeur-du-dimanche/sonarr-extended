using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class release_upstream_migration_versionsFixture : MigrationTest<release_upstream_migration_versions>
    {
        private const long LastUpstreamMigrationBeforeFork = 233;

        // Reproduces a database migrated by a fork release that still numbered its migrations 234-237
        private IDirectDataMapper WithForkMigrationsAppliedWithUpstreamVersions()
        {
            var db = WithTestDb(new MigrationContext(MigrationType, LastUpstreamMigrationBeforeFork)
            {
                BeforeMigration = m =>
                {
                    if (m is quality_profile_quality_ranks migration)
                    {
                        GivenForkMigrationsAppliedWithUpstreamVersions(migration);
                    }
                }
            });

            var connection = Mocker.Resolve<IConnectionStringFactory>().MainDbConnection;
            Mocker.Resolve<IMigrationController>().Migrate(connection.ConnectionString, new MigrationContext(MigrationType), connection.DatabaseType);

            return db.GetDirectDataMapper();
        }

        private static void GivenForkMigrationsAppliedWithUpstreamVersions(NzbDroneMigrationBase c)
        {
            c.Alter.Table("Series")
                .AddColumn("EpisodeOrder").AsInt32().WithDefaultValue(0)
                .AddColumn("TmdbEpisodeGroupId").AsString().Nullable()
                .AddColumn("SearchByAbsoluteNumber").AsBoolean().WithDefaultValue(false);

            c.Create.TableForModel("SeriesAliases")
                .WithColumn("SeriesId").AsInt32().NotNullable().Indexed()
                .WithColumn("Title").AsString().NotNullable()
                .WithColumn("SeasonNumber").AsInt32().Nullable()
                .WithColumn("SceneSeasonNumber").AsInt32().Nullable();

            c.Insert.IntoTable("VersionInfo")
                .Row(new { Version = 234L, AppliedOn = DateTime.UtcNow, Description = "add_series_episode_order" })
                .Row(new { Version = 235L, AppliedOn = DateTime.UtcNow, Description = "add_series_aliases" })
                .Row(new { Version = 236L, AppliedOn = DateTime.UtcNow, Description = "add_season_to_series_aliases" })
                .Row(new { Version = 237L, AppliedOn = DateTime.UtcNow, Description = "add_series_search_by_absolute_number" });

            c.Insert.IntoTable("Series").Row(new
            {
                TvdbId = 1,
                TvRageId = 1,
                TvMazeId = 1,
                TmdbId = 1,
                Title = "Title1",
                CleanTitle = "CleanTitle1",
                TitleSlug = "title1",
                Status = 1,
                Images = "[]",
                Path = "c:\\test",
                Monitored = true,
                SeasonFolder = true,
                Runtime = 0,
                SeriesType = 0,
                UseSceneNumbering = false,
                OriginalLanguage = 1,
                QualityProfileId = 1,
                EpisodeOrder = 1,
                TmdbEpisodeGroupId = "group-id",
                SearchByAbsoluteNumber = true
            });

            c.Insert.IntoTable("SeriesAliases").Row(new
            {
                SeriesId = 1,
                Title = "Alias",
                SeasonNumber = 2,
                SceneSeasonNumber = 3
            });
        }

        private static string GetDescription(IDirectDataMapper db, long version)
        {
            return db.Query("SELECT * FROM \"VersionInfo\"")
                .Where(v => Convert.ToInt64(v["Version"]) == version)
                .Select(v => (string)v["Description"])
                .SingleOrDefault();
        }

        [Test]
        public void should_keep_data_when_fork_migrations_were_applied_with_upstream_versions()
        {
            var db = WithForkMigrationsAppliedWithUpstreamVersions();

            var series = db.Query("SELECT * FROM \"Series\"").Single();

            Convert.ToInt32(series["EpisodeOrder"]).Should().Be(1);
            series["TmdbEpisodeGroupId"].Should().Be("group-id");
            Convert.ToBoolean(series["SearchByAbsoluteNumber"]).Should().BeTrue();

            var alias = db.Query("SELECT * FROM \"SeriesAliases\"").Single();

            alias["Title"].Should().Be("Alias");
            Convert.ToInt32(alias["SeasonNumber"]).Should().Be(2);
            Convert.ToInt32(alias["SceneSeasonNumber"]).Should().Be(3);
        }

        [Test]
        public void should_run_upstream_migrations_reusing_versions_previously_used_by_the_fork()
        {
            var db = WithForkMigrationsAppliedWithUpstreamVersions();

            GetDescription(db, 234).Should().Be(nameof(add_pending_path_to_series));
            GetDescription(db, 236).Should().Be(nameof(import_list_tag_existing_type));
        }

        [Test]
        public void should_record_fork_migrations_with_their_own_versions()
        {
            var db = WithForkMigrationsAppliedWithUpstreamVersions();

            GetDescription(db, 235).Should().BeNull();
            GetDescription(db, 237).Should().BeNull();
            GetDescription(db, 10001).Should().Be(nameof(add_series_episode_order));
            GetDescription(db, 10004).Should().Be(nameof(add_series_search_by_absolute_number));
        }

        [Test]
        public void should_create_fork_schema_on_database_without_it()
        {
            var db = WithTestDb(new MigrationContext(MigrationType)).GetDirectDataMapper();

            db.Query("SELECT \"EpisodeOrder\", \"TmdbEpisodeGroupId\", \"SearchByAbsoluteNumber\" FROM \"Series\"").Should().BeEmpty();
            db.Query("SELECT \"SeriesId\", \"Title\", \"SeasonNumber\", \"SceneSeasonNumber\" FROM \"SeriesAliases\"").Should().BeEmpty();
            GetDescription(db, 236).Should().Be(nameof(import_list_tag_existing_type));
        }
    }
}
