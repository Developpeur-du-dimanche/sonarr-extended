using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Tv.Aliases
{
    public interface ISeriesAliasService
    {
        List<SeriesAlias> GetBySeriesId(int seriesId);
        Dictionary<int, List<SeriesAlias>> GetAllBySeriesId();
        List<SeriesAliasWithTvdbId> GetAllWithTvdbId();
        void SetAliases(int seriesId, IEnumerable<SeriesAlias> aliases);
    }

    public class SeriesAliasService : ISeriesAliasService, IHandle<SeriesDeletedEvent>
    {
        private readonly ISeriesAliasRepository _repository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public SeriesAliasService(ISeriesAliasRepository repository, IEventAggregator eventAggregator, Logger logger)
        {
            _repository = repository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<SeriesAlias> GetBySeriesId(int seriesId)
        {
            return _repository.GetBySeriesId(seriesId);
        }

        public Dictionary<int, List<SeriesAlias>> GetAllBySeriesId()
        {
            return _repository.All()
                              .GroupBy(a => a.SeriesId)
                              .ToDictionary(g => g.Key, g => g.ToList());
        }

        public List<SeriesAliasWithTvdbId> GetAllWithTvdbId()
        {
            return _repository.AllWithTvdbId();
        }

        public void SetAliases(int seriesId, IEnumerable<SeriesAlias> aliases)
        {
            var newAliases = aliases.Where(a => a.Title.IsNotNullOrWhiteSpace())
                                    .Select(a => Normalize(seriesId, a))
                                    .DistinctBy(a => (a.Title.ToLowerInvariant(), a.SeasonNumber, a.SceneSeasonNumber))
                                    .ToList();

            var existing = _repository.GetBySeriesId(seriesId);

            var toDelete = existing.Where(e => !newAliases.Any(n => IsSameAlias(e, n))).ToList();
            var toInsert = newAliases.Where(n => !existing.Any(e => IsSameAlias(e, n))).ToList();

            if (toDelete.Empty() && toInsert.Empty())
            {
                return;
            }

            _logger.Debug("Updating aliases for series {0}: {1} added, {2} removed", seriesId, toInsert.Count, toDelete.Count);

            _repository.DeleteMany(toDelete);
            _repository.InsertMany(toInsert);

            _eventAggregator.PublishEvent(new SeriesAliasesUpdatedEvent(new List<int> { seriesId }));
        }

        public void Handle(SeriesDeletedEvent message)
        {
            var seriesIds = message.Series.Select(s => s.Id).ToList();

            _repository.DeleteForSeries(seriesIds);

            _eventAggregator.PublishEvent(new SeriesAliasesUpdatedEvent(seriesIds));
        }

        private static SeriesAlias Normalize(int seriesId, SeriesAlias alias)
        {
            var seasonNumber = alias.SeasonNumber >= 0 ? alias.SeasonNumber : null;

            // The release season only matters for a specific season, and only when it differs from it.
            var sceneSeasonNumber = seasonNumber.HasValue && alias.SceneSeasonNumber >= 0 && alias.SceneSeasonNumber != seasonNumber
                ? alias.SceneSeasonNumber
                : null;

            return new SeriesAlias
            {
                SeriesId = seriesId,
                Title = alias.Title.Trim(),
                SeasonNumber = seasonNumber,
                SceneSeasonNumber = sceneSeasonNumber
            };
        }

        private static bool IsSameAlias(SeriesAlias first, SeriesAlias second)
        {
            return first.Title == second.Title &&
                   first.SeasonNumber == second.SeasonNumber &&
                   first.SceneSeasonNumber == second.SceneSeasonNumber;
        }
    }
}
