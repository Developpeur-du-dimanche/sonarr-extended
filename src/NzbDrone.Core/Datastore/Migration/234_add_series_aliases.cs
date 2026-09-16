using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(234)]
    public class add_series_aliases : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("SeriesAliases")
                .WithColumn("SeriesId").AsInt32().NotNullable().Indexed()
                .WithColumn("Title").AsString().NotNullable();
        }
    }
}
