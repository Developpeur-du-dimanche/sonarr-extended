using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Tv.Aliases;

namespace NzbDrone.Core.Test.TvTests.SeriesAliasTests
{
    [TestFixture]
    public class SeriesAliasRepositoryFixture : DbTest<SeriesAliasRepository, SeriesAlias>
    {
        private Series _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew()
                                     .With(s => s.TvdbId = 12345)
                                     .BuildNew();

            Db.Insert(_series);
        }

        [Test]
        public void should_return_titles_with_series_tvdb_id()
        {
            Subject.Insert(new SeriesAlias { SeriesId = _series.Id, Title = "My Alias" });

            Subject.AllTitlesByTvdbId().Should().BeEquivalentTo(new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(12345, "My Alias")
            });
        }

        [Test]
        public void should_delete_aliases_for_series()
        {
            Subject.Insert(new SeriesAlias { SeriesId = _series.Id, Title = "My Alias" });
            Subject.Insert(new SeriesAlias { SeriesId = _series.Id + 1, Title = "Other Alias" });

            Subject.DeleteForSeries(new List<int> { _series.Id });

            Subject.All().Should().OnlyContain(a => a.Title == "Other Alias");
        }
    }
}
