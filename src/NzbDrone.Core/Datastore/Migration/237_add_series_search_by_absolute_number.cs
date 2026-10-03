using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(237)]
    public class add_series_search_by_absolute_number : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("Series")
                .AddColumn("SearchByAbsoluteNumber").AsBoolean().WithDefaultValue(false);
        }
    }
}
