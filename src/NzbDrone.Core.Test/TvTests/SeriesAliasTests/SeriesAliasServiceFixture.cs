using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Aliases;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.TvTests.SeriesAliasTests
{
    [TestFixture]
    public class SeriesAliasServiceFixture : CoreTest<SeriesAliasService>
    {
        private List<SeriesAlias> _existing;

        [SetUp]
        public void Setup()
        {
            _existing = new List<SeriesAlias>
            {
                new SeriesAlias { Id = 1, SeriesId = 5, Title = "First Alias" },
                new SeriesAlias { Id = 2, SeriesId = 5, Title = "Second Alias" }
            };

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Setup(s => s.GetBySeriesId(5))
                  .Returns(_existing);
        }

        [Test]
        public void should_insert_new_and_delete_removed_aliases()
        {
            Subject.SetAliases(5, new List<string> { "First Alias", " Third Alias " });

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.DeleteMany(It.Is<List<SeriesAlias>>(l => l.Count == 1 && l[0].Id == 2)), Times.Once());

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.InsertMany(It.Is<IList<SeriesAlias>>(l => l.Count == 1 && l[0].Title == "Third Alias" && l[0].SeriesId == 5)), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesAliasesUpdatedEvent>()), Times.Once());
        }

        [Test]
        public void should_ignore_empty_and_duplicate_aliases()
        {
            Subject.SetAliases(5, new List<string> { "First Alias", "Second Alias", "second alias", "", "  " });

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.InsertMany(It.IsAny<IList<SeriesAlias>>()), Times.Never());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesAliasesUpdatedEvent>()), Times.Never());
        }

        [Test]
        public void should_delete_aliases_when_series_is_deleted()
        {
            var series = new List<Series> { new Series { Id = 5 }, new Series { Id = 6 } };

            Subject.Handle(new SeriesDeletedEvent(series, false, false));

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.DeleteForSeries(It.Is<List<int>>(l => l.SequenceEqual(new[] { 5, 6 }))), Times.Once());
        }
    }
}
