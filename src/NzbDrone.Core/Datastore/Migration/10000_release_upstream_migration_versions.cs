using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    // Fork migrations are numbered from 10000 so they never collide with upstream's sequence.
    // Superseded by MigrationController releasing these versions before migrating, kept as databases already recorded it.
    [Migration(10000)]
    public class release_upstream_migration_versions : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            foreach (var (version, description) in ForkMigrationVersions.ReleasedUpstreamVersions)
            {
                Delete.FromTable("VersionInfo").Row(new { Version = version, Description = description });
            }
        }
    }
}
