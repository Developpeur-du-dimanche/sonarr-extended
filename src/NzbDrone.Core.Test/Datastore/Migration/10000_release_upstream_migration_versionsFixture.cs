using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore.Migration;
using NzbDrone.Core.Datastore.Migration.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Datastore.Migration
{
    [TestFixture]
    public class release_upstream_migration_versionsFixture : MigrationTest<release_upstream_migration_versions>
    {
        private const long LastForkMigration = 10004;

        private IDirectDataMapper WithAllForkMigrations(Action<release_upstream_migration_versions> beforeMigration = null)
        {
            return WithTestDb(new MigrationContext(MigrationType, LastForkMigration)
            {
                BeforeMigration = m =>
                {
                    if (beforeMigration != null && m is release_upstream_migration_versions migration)
                    {
                        beforeMigration(migration);
                    }
                }
            }).GetDirectDataMapper();
        }

        private static void GivenForkMigrationsAppliedWithUpstreamVersions(release_upstream_migration_versions c)
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

        [Test]
        public void should_keep_data_when_fork_migrations_were_applied_with_upstream_versions()
        {
            var db = WithAllForkMigrations(GivenForkMigrationsAppliedWithUpstreamVersions);

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
        public void should_release_upstream_versions_used_by_fork_migrations()
        {
            var db = WithAllForkMigrations(GivenForkMigrationsAppliedWithUpstreamVersions);

            var versions = db.Query("SELECT * FROM \"VersionInfo\"").Select(v => Convert.ToInt64(v["Version"])).ToList();

            versions.Should().NotContain(new[] { 234L, 235L, 236L, 237L });
            versions.Should().Contain(new[] { 10000L, 10001L, 10002L, 10003L, 10004L });
        }

        [Test]
        public void should_not_release_version_applied_by_an_upstream_migration()
        {
            var db = WithAllForkMigrations(c =>
            {
                c.Insert.IntoTable("VersionInfo")
                    .Row(new { Version = 234L, AppliedOn = DateTime.UtcNow, Description = "upstream_migration" });
            });

            var versions = db.Query("SELECT * FROM \"VersionInfo\"").Select(v => Convert.ToInt64(v["Version"])).ToList();

            versions.Should().Contain(234L);
        }

        [Test]
        public void should_create_fork_schema_on_database_without_it()
        {
            var db = WithAllForkMigrations();

            db.Query("SELECT \"EpisodeOrder\", \"TmdbEpisodeGroupId\", \"SearchByAbsoluteNumber\" FROM \"Series\"").Should().BeEmpty();
            db.Query("SELECT \"SeriesId\", \"Title\", \"SeasonNumber\", \"SceneSeasonNumber\" FROM \"SeriesAliases\"").Should().BeEmpty();
        }

        [Test]
        public void should_record_migrations_with_the_description_used_to_release_versions()
        {
            var db = WithAllForkMigrations();

            var applied = db.Query("SELECT * FROM \"VersionInfo\"").Single(v => Convert.ToInt64(v["Version"]) == 10001L);

            applied["Description"].Should().Be(nameof(add_series_episode_order));
        }
    }
}
