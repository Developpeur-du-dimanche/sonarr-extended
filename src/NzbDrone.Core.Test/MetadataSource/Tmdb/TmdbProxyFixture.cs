using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource.Tmdb;
using NzbDrone.Core.MetadataSource.Tmdb.Resource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource.Tmdb
{
    [TestFixture]
    public class TmdbProxyFixture : CoreTest<TmdbProxy>
    {
        [SetUp]
        public void Setup()
        {
            GivenApiKey("abc123");
        }

        private void GivenApiKey(string apiKey)
        {
            Mocker.GetMock<IConfigService>()
                  .SetupGet(s => s.TmdbApiKey)
                  .Returns(apiKey);
        }

        private void GivenResponse<T>(string resource, string json)
            where T : new()
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(s => s.Get<T>(It.Is<HttpRequest>(r => r.Url.Path.EndsWith(resource))))
                  .Returns<HttpRequest>(r => new HttpResponse<T>(new HttpResponse(r, new HttpHeader(), json)));
        }

        [Test]
        public void should_throw_if_api_key_is_not_configured()
        {
            GivenApiKey(string.Empty);

            Assert.Throws<TmdbException>(() => Subject.GetEpisodes(1399, null));
        }

        [Test]
        public void should_get_episodes_for_each_season_for_default_order()
        {
            GivenResponse<TmdbShowResource>("tv/1399", "{\"id\":1399,\"seasons\":[{\"season_number\":0},{\"season_number\":1}]}");
            GivenResponse<TmdbSeasonResource>("tv/1399/season/0", "{\"season_number\":0,\"episodes\":[{\"season_number\":0,\"episode_number\":1,\"name\":\"Special\",\"air_date\":\"2010-12-05\"}]}");
            GivenResponse<TmdbSeasonResource>("tv/1399/season/1", "{\"season_number\":1,\"episodes\":[{\"season_number\":1,\"episode_number\":1,\"name\":\"Pilot\",\"air_date\":\"2011-04-17\",\"runtime\":62,\"episode_type\":\"standard\",\"still_path\":\"/pilot.jpg\"},{\"season_number\":1,\"episode_number\":2,\"name\":\"Finale\",\"air_date\":\"\",\"episode_type\":\"finale\"}]}");

            var episodes = Subject.GetEpisodes(1399, null);

            episodes.Should().HaveCount(3);

            var pilot = episodes.Single(e => e.SeasonNumber == 1 && e.EpisodeNumber == 1);
            pilot.Title.Should().Be("Pilot");
            pilot.AirDate.Should().Be("2011-04-17");
            pilot.Runtime.Should().Be(62);
            pilot.FinaleType.Should().BeNull();
            pilot.Images.Should().ContainSingle(i => i.RemoteUrl == "https://image.tmdb.org/t/p/original/pilot.jpg");

            var finale = episodes.Single(e => e.SeasonNumber == 1 && e.EpisodeNumber == 2);
            finale.AirDate.Should().BeNull();
            finale.FinaleType.Should().Be("season");
        }

        [Test]
        public void should_use_episode_group_order_for_season_and_episode_numbers()
        {
            GivenResponse<TmdbEpisodeGroupDetailsResource>("tv/episode_group/5b11ba820e0a265847002c6e", "{\"id\":\"5b11ba820e0a265847002c6e\",\"groups\":[{\"name\":\"Volume 1\",\"order\":1,\"episodes\":[{\"season_number\":1,\"episode_number\":3,\"order\":0,\"name\":\"C\"},{\"season_number\":1,\"episode_number\":1,\"order\":1,\"name\":\"A\"}]},{\"name\":\"Volume 2\",\"order\":2,\"episodes\":[{\"season_number\":1,\"episode_number\":2,\"order\":0,\"name\":\"B\"}]}]}");

            var episodes = Subject.GetEpisodes(1399, "5b11ba820e0a265847002c6e");

            episodes.Select(e => $"{e.SeasonNumber}x{e.EpisodeNumber} {e.Title}")
                    .Should().Equal("1x1 C", "1x2 A", "2x1 B");

            Mocker.GetMock<IHttpClient>()
                  .Verify(v => v.Get<TmdbShowResource>(It.IsAny<HttpRequest>()), Times.Never());
        }

        [Test]
        public void should_pass_v3_api_key_as_query_parameter()
        {
            GivenResponse<TmdbEpisodeGroupListResource>("tv/1399/episode_groups", "{\"results\":[]}");

            Subject.GetEpisodeGroups(1399);

            Mocker.GetMock<IHttpClient>()
                  .Verify(v => v.Get<TmdbEpisodeGroupListResource>(It.Is<HttpRequest>(r => r.Url.Query.Contains("api_key=abc123") && r.Headers.GetSingleValue("Authorization") == null)), Times.Once());
        }

        [Test]
        public void should_pass_read_access_token_as_bearer_header()
        {
            GivenApiKey("eyJhbGciOiJIUzI1NiJ9.token");
            GivenResponse<TmdbEpisodeGroupListResource>("tv/1399/episode_groups", "{\"results\":[{\"id\":\"abc\",\"name\":\"DVD Order\",\"type\":3,\"episode_count\":24,\"group_count\":2}]}");

            var groups = Subject.GetEpisodeGroups(1399);

            groups.Should().ContainSingle();
            groups.First().Type.Should().Be(TmdbEpisodeGroupType.Dvd);
            groups.First().EpisodeCount.Should().Be(24);

            Mocker.GetMock<IHttpClient>()
                  .Verify(v => v.Get<TmdbEpisodeGroupListResource>(It.Is<HttpRequest>(r => !r.Url.Query.Contains("api_key") && r.Headers.GetSingleValue("Authorization") == "Bearer eyJhbGciOiJIUzI1NiJ9.token")), Times.Once());
        }
    }
}
