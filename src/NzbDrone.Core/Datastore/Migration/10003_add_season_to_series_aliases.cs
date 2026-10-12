using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(10003)]
    public class add_season_to_series_aliases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (!Schema.Table("SeriesAliases").Column("SeasonNumber").Exists())
            {
                Alter.Table("SeriesAliases").AddColumn("SeasonNumber").AsInt32().Nullable();
            }

            if (!Schema.Table("SeriesAliases").Column("SceneSeasonNumber").Exists())
            {
                Alter.Table("SeriesAliases").AddColumn("SceneSeasonNumber").AsInt32().Nullable();
            }
        }
    }
}
