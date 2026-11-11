using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(10004)]
    public class add_series_search_by_absolute_number : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            if (Schema.Table("Series").Column("SearchByAbsoluteNumber").Exists())
            {
                return;
            }

            Alter.Table("Series")
                .AddColumn("SearchByAbsoluteNumber").AsBoolean().WithDefaultValue(false);
        }
    }
}
