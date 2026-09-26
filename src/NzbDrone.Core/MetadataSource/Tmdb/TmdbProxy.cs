using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MetadataSource.Tmdb.Resource;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public interface ITmdbProxy
    {
        bool IsConfigured { get; }
        List<Episode> GetEpisodes(int tmdbId, string episodeGroupId);
        List<TmdbEpisodeGroup> GetEpisodeGroups(int tmdbId);
    }

    public class TmdbProxy : ITmdbProxy
    {
        private const string BaseUrl = "https://api.themoviedb.org/3/";
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/original";

        private readonly IHttpClient _httpClient;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public TmdbProxy(IHttpClient httpClient, IConfigService configService, Logger logger)
        {
            _httpClient = httpClient;
            _configService = configService;
            _logger = logger;
        }

        public bool IsConfigured => _configService.TmdbApiKey.IsNotNullOrWhiteSpace();

        public List<Episode> GetEpisodes(int tmdbId, string episodeGroupId)
        {
            var episodes = episodeGroupId.IsNotNullOrWhiteSpace()
                ? GetEpisodeGroupEpisodes(episodeGroupId)
                : GetShowEpisodes(tmdbId);

            SetAbsoluteEpisodeNumbers(episodes);

            return episodes;
        }

        public List<TmdbEpisodeGroup> GetEpisodeGroups(int tmdbId)
        {
            var groups = Execute<TmdbEpisodeGroupListResource>($"tv/{tmdbId}/episode_groups");

            return [.. groups.Results.Select(g => new TmdbEpisodeGroup
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                Type = (TmdbEpisodeGroupType)g.Type,
                EpisodeCount = g.EpisodeCount,
                GroupCount = g.GroupCount
            })];
        }

        private List<Episode> GetShowEpisodes(int tmdbId)
        {
            var show = Execute<TmdbShowResource>($"tv/{tmdbId}");
            var episodes = new List<Episode>();

            foreach (var seasonSummary in show.Seasons)
            {
                var season = Execute<TmdbSeasonResource>($"tv/{tmdbId}/season/{seasonSummary.SeasonNumber}");

                episodes.AddRange(season.Episodes.Select(e => MapEpisode(e, e.SeasonNumber, e.EpisodeNumber)));
            }

            return episodes;
        }

        private List<Episode> GetEpisodeGroupEpisodes(string episodeGroupId)
        {
            var details = Execute<TmdbEpisodeGroupDetailsResource>($"tv/episode_group/{episodeGroupId}");

            // In an episode group each group is a season, its order is the season number
            // and the order of an episode within the group is its (zero based) episode number.
            return [.. details.Groups.SelectMany(g => g.Episodes.Select(e => MapEpisode(e, g.Order, e.Order + 1)))];
        }

        private void SetAbsoluteEpisodeNumbers(List<Episode> episodes)
        {
            var absoluteEpisodeNumber = 1;

            foreach (var episode in episodes.Where(e => e.SeasonNumber > 0)
                                            .OrderBy(e => e.SeasonNumber)
                                            .ThenBy(e => e.EpisodeNumber))
            {
                episode.AbsoluteEpisodeNumber = absoluteEpisodeNumber++;
            }

            if (_logger.IsDebugEnabled)
            {
                var ranges = episodes.Where(e => e.AbsoluteEpisodeNumber.HasValue)
                                     .GroupBy(e => e.SeasonNumber)
                                     .OrderBy(g => g.Key)
                                     .Select(g => $"S{g.Key:00}: {g.Min(e => e.AbsoluteEpisodeNumber)}-{g.Max(e => e.AbsoluteEpisodeNumber)}");

                _logger.Debug("Derived absolute episode numbers from the TMDB episode order ({0})", string.Join(", ", ranges));
            }
        }

        private T Execute<T>(string resource)
            where T : new()
        {
            var apiKey = _configService.TmdbApiKey;

            if (apiKey.IsNullOrWhiteSpace())
            {
                throw new TmdbException("TMDB API key is not configured");
            }

            var requestBuilder = new HttpRequestBuilder(BaseUrl)
                .Resource(resource)
                .Accept(HttpAccept.Json);

            // v4 read access tokens are JWTs, v3 API keys are passed as a query parameter
            if (apiKey.StartsWith("eyJ", StringComparison.OrdinalIgnoreCase))
            {
                requestBuilder.SetHeader("Authorization", $"Bearer {apiKey}");
            }
            else
            {
                requestBuilder.AddQueryParam("api_key", apiKey);
            }

            var request = requestBuilder.Build();
            request.SuppressHttpError = true;

            try
            {
                var response = _httpClient.Get<T>(request);

                if (response.HasHttpError)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new TmdbException("TMDB resource '{0}' was not found", resource);
                    }

                    throw new HttpException(request, response);
                }

                return response.Resource;
            }
            catch (TmdbException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to communicate with TMDB");
                throw new TmdbException("Unable to communicate with TMDB. {0}", ex, ex.Message);
            }
        }

        private static Episode MapEpisode(TmdbEpisodeResource resource, int seasonNumber, int episodeNumber)
        {
            var episode = new Episode
            {
                SeasonNumber = seasonNumber,
                EpisodeNumber = episodeNumber,
                Title = resource.Name,
                Overview = resource.Overview,
                AirDate = resource.AirDate.IsNotNullOrWhiteSpace() ? resource.AirDate : null,
                Runtime = resource.Runtime ?? 0,
                FinaleType = MapFinaleType(resource.EpisodeType),
                Ratings = new Ratings
                {
                    Value = resource.VoteAverage,
                    Votes = resource.VoteCount
                }
            };

            if (resource.StillPath.IsNotNullOrWhiteSpace())
            {
                episode.Images.Add(new MediaCover.MediaCover(MediaCoverTypes.Screenshot, ImageBaseUrl + resource.StillPath));
            }

            return episode;
        }

        private static string MapFinaleType(string episodeType)
        {
            switch (episodeType)
            {
                case "finale":
                    return "season";
                case "mid_season":
                    return "midseason";
                default:
                    return null;
            }
        }
    }
}
