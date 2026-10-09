using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(234)]
    public class add_series_episode_order : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("EpisodeOrder").AsInt32().WithDefaultValue(0)
                .AddColumn("TmdbEpisodeGroupId").AsString().Nullable();
        }
    }
}
