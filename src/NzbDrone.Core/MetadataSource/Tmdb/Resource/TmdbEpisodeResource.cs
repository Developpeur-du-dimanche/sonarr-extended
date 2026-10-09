using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.Tmdb.Resource
{
    public class TmdbEpisodeResource
    {
        [JsonProperty("season_number")]
        public int SeasonNumber { get; set; }

        [JsonProperty("episode_number")]
        public int EpisodeNumber { get; set; }

        // Only set for episodes that are part of an episode group
        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("air_date")]
        public string AirDate { get; set; }

        [JsonProperty("runtime")]
        public int? Runtime { get; set; }

        [JsonProperty("episode_type")]
        public string EpisodeType { get; set; }

        [JsonProperty("still_path")]
        public string StillPath { get; set; }

        [JsonProperty("vote_average")]
        public decimal VoteAverage { get; set; }

        [JsonProperty("vote_count")]
        public int VoteCount { get; set; }
    }
}
