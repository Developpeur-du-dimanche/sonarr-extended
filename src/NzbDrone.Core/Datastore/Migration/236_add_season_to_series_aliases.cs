using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(236)]
    public class add_season_to_series_aliases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("SeriesAliases")
                .AddColumn("SeasonNumber").AsInt32().Nullable()
                .AddColumn("SceneSeasonNumber").AsInt32().Nullable();
        }
    }
}
