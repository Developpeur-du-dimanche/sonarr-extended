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
            Subject.SetAliases(5, new List<SeriesAlias> { Alias("First Alias"), Alias(" Third Alias ") });

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
            Subject.SetAliases(5, new List<SeriesAlias> { Alias("First Alias"), Alias("Second Alias"), Alias("second alias"), Alias(""), Alias("  ") });

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.InsertMany(It.IsAny<IList<SeriesAlias>>()), Times.Never());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<SeriesAliasesUpdatedEvent>()), Times.Never());
        }

        [Test]
        public void should_replace_alias_when_its_season_changes()
        {
            Subject.SetAliases(5, new List<SeriesAlias> { Alias("First Alias", 2), Alias("Second Alias") });

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.DeleteMany(It.Is<List<SeriesAlias>>(l => l.Count == 1 && l[0].Id == 1)), Times.Once());

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.InsertMany(It.Is<IList<SeriesAlias>>(l => l.Count == 1 && l[0].Title == "First Alias" && l[0].SeasonNumber == 2)), Times.Once());
        }

        [Test]
        public void should_keep_same_title_for_different_seasons()
        {
            Subject.SetAliases(5, new List<SeriesAlias> { Alias("First Alias"), Alias("Second Alias"), Alias("Season Alias", 1), Alias("Season Alias", 2) });

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.InsertMany(It.Is<IList<SeriesAlias>>(l => l.Count == 2 && l.All(a => a.Title == "Season Alias"))), Times.Once());
        }

        [TestCase(4, 1, 4, 1)]
        [TestCase(4, 4, 4, null)]
        [TestCase(4, null, 4, null)]
        [TestCase(null, 1, null, null)]
        [TestCase(-1, 1, null, null)]
        public void should_only_keep_release_season_when_it_differs_from_season(int? seasonNumber, int? sceneSeasonNumber, int? expectedSeasonNumber, int? expectedSceneSeasonNumber)
        {
            Subject.SetAliases(5, new List<SeriesAlias> { Alias("First Alias"), Alias("Second Alias"), Alias("Season Alias", seasonNumber, sceneSeasonNumber) });

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.InsertMany(It.Is<IList<SeriesAlias>>(l => l.Count == 1 && l[0].SeasonNumber == expectedSeasonNumber && l[0].SceneSeasonNumber == expectedSceneSeasonNumber)), Times.Once());
        }

        [Test]
        public void should_delete_aliases_when_series_is_deleted()
        {
            var series = new List<Series> { new Series { Id = 5 }, new Series { Id = 6 } };

            Subject.Handle(new SeriesDeletedEvent(series, false, false));

            Mocker.GetMock<ISeriesAliasRepository>()
                  .Verify(v => v.DeleteForSeries(It.Is<List<int>>(l => l.SequenceEqual(new[] { 5, 6 }))), Times.Once());
        }

        private static SeriesAlias Alias(string title, int? seasonNumber = null, int? sceneSeasonNumber = null)
        {
            return new SeriesAlias { Title = title, SeasonNumber = seasonNumber, SceneSeasonNumber = sceneSeasonNumber };
        }
    }
}
