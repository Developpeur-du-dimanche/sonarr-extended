using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Commands;
using NzbDrone.Core.Tv.Events;

namespace NzbDrone.Core.Test.TvTests
{
    [TestFixture]
    public class SeriesEditedServiceFixture : CoreTest<SeriesEditedService>
    {
        private Series _oldSeries;
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _oldSeries = Builder<Series>.CreateNew()
                .With(s => s.SeasonType = SeasonType.Official)
                .Build();

            _series = _oldSeries.JsonClone();
        }

        private void VerifyRefresh(Times times)
        {
            Mocker.GetMock<IManageCommandQueue>()
                  .Verify(v => v.Push(It.IsAny<RefreshSeriesCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()), times);
        }

        [Test]
        public void should_not_refresh_series_when_nothing_relevant_changed()
        {
            _series.Monitored = !_oldSeries.Monitored;

            Subject.Handle(new SeriesEditedEvent(_series, _oldSeries));

            VerifyRefresh(Times.Never());
        }

        [Test]
        public void should_refresh_series_when_season_type_changed()
        {
            _series.SeasonType = "dvd";

            Subject.Handle(new SeriesEditedEvent(_series, _oldSeries));

            VerifyRefresh(Times.Once());
        }

        [Test]
        public void should_refresh_series_when_episode_order_changed()
        {
            _series.EpisodeOrder = EpisodeOrderType.Tmdb;

            Subject.Handle(new SeriesEditedEvent(_series, _oldSeries));

            VerifyRefresh(Times.Once());
        }
    }
}
