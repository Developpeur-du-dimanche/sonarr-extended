using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Tv.Aliases
{
    public interface ISeriesAliasRepository : IBasicRepository<SeriesAlias>
    {
        List<SeriesAlias> GetBySeriesId(int seriesId);
        List<KeyValuePair<int, string>> AllTitlesByTvdbId();
        void DeleteForSeries(List<int> seriesIds);
    }

    public class SeriesAliasRepository : BasicRepository<SeriesAlias>, ISeriesAliasRepository
    {
        public SeriesAliasRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<SeriesAlias> GetBySeriesId(int seriesId)
        {
            return Query(x => x.SeriesId == seriesId);
        }

        public List<KeyValuePair<int, string>> AllTitlesByTvdbId()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Series\".\"TvdbId\" AS Key, \"SeriesAliases\".\"Title\" AS Value FROM \"SeriesAliases\" INNER JOIN \"Series\" ON \"Series\".\"Id\" = \"SeriesAliases\".\"SeriesId\"";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToList();
            }
        }

        public void DeleteForSeries(List<int> seriesIds)
        {
            Delete(x => seriesIds.Contains(x.SeriesId));
        }
    }
}
