using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(10001)]
    public class add_series_episode_order : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Table("Series").Column("EpisodeOrder").Exists())
            {
                Alter.Table("Series").AddColumn("EpisodeOrder").AsInt32().WithDefaultValue(0);
            }

            if (!Schema.Table("Series").Column("TmdbEpisodeGroupId").Exists())
            {
                Alter.Table("Series").AddColumn("TmdbEpisodeGroupId").AsString().Nullable();
            }
        }
    }
}
