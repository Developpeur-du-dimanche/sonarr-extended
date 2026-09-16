using System;
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
        Dictionary<int, List<string>> GetAllTitlesBySeriesId();
        List<KeyValuePair<int, string>> GetAllTitlesByTvdbId();
        void SetAliases(int seriesId, IEnumerable<string> titles);
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

        public Dictionary<int, List<string>> GetAllTitlesBySeriesId()
        {
            return _repository.All()
                              .GroupBy(a => a.SeriesId)
                              .ToDictionary(g => g.Key, g => g.Select(a => a.Title).ToList());
        }

        public List<KeyValuePair<int, string>> GetAllTitlesByTvdbId()
        {
            return _repository.AllTitlesByTvdbId();
        }

        public void SetAliases(int seriesId, IEnumerable<string> titles)
        {
            var newTitles = titles.Where(t => t.IsNotNullOrWhiteSpace())
                                  .Select(t => t.Trim())
                                  .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                  .ToList();

            var existing = _repository.GetBySeriesId(seriesId);

            var toDelete = existing.Where(a => !newTitles.Contains(a.Title, StringComparer.InvariantCulture)).ToList();
            var toInsert = newTitles.Where(t => !existing.Any(a => a.Title == t))
                                    .Select(t => new SeriesAlias { SeriesId = seriesId, Title = t })
                                    .ToList();

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
    }
}
