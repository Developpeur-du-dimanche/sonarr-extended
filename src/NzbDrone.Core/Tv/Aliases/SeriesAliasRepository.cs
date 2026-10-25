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
        List<SeriesAliasWithTvdbId> AllWithTvdbId();
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

        public List<SeriesAliasWithTvdbId> AllWithTvdbId()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT \"Series\".\"TvdbId\", \"SeriesAliases\".\"Title\", \"SeriesAliases\".\"SeasonNumber\", \"SeriesAliases\".\"SceneSeasonNumber\" FROM \"SeriesAliases\" INNER JOIN \"Series\" ON \"Series\".\"Id\" = \"SeriesAliases\".\"SeriesId\"";
                return conn.Query<SeriesAliasWithTvdbId>(strSql).ToList();
            }
        }

        public void DeleteForSeries(List<int> seriesIds)
        {
            Delete(x => seriesIds.Contains(x.SeriesId));
        }
    }
}
