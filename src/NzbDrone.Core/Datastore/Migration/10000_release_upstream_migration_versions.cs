using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    // Fork migrations are numbered from 10000 so they never collide with upstream's sequence.
    [Migration(10000)]
    public class release_upstream_migration_versions : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            // These were applied as 234-237 before the fork got its own range. The description is matched as well
            // so an upstream migration that later takes one of these versions is left alone.
            Delete.FromTable("VersionInfo")
                .Row(new { Version = 234L, Description = nameof(add_series_episode_order) })
                .Row(new { Version = 235L, Description = nameof(add_series_aliases) })
                .Row(new { Version = 236L, Description = nameof(add_season_to_series_aliases) })
                .Row(new { Version = 237L, Description = nameof(add_series_search_by_absolute_number) });
        }
    }
}
