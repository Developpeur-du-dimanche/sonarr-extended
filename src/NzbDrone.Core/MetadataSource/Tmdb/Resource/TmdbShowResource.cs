using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.Tmdb.Resource
{
    public class TmdbShowResource
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("seasons")]
        public List<TmdbSeasonSummaryResource> Seasons { get; set; } = new();
    }

    public class TmdbSeasonSummaryResource
    {
        [JsonProperty("season_number")]
        public int SeasonNumber { get; set; }
    }

    public class TmdbSeasonResource
    {
        [JsonProperty("season_number")]
        public int SeasonNumber { get; set; }

        [JsonProperty("episodes")]
        public List<TmdbEpisodeResource> Episodes { get; set; } = new();
    }
}
