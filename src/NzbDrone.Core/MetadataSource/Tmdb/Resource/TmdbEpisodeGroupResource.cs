using System.Collections.Generic;
using Newtonsoft.Json;

namespace NzbDrone.Core.MetadataSource.Tmdb.Resource
{
    public class TmdbEpisodeGroupListResource
    {
        [JsonProperty("results")]
        public List<TmdbEpisodeGroupResource> Results { get; set; } = new();
    }

    public class TmdbEpisodeGroupResource
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("episode_count")]
        public int EpisodeCount { get; set; }

        [JsonProperty("group_count")]
        public int GroupCount { get; set; }
    }

    public class TmdbEpisodeGroupDetailsResource
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("groups")]
        public List<TmdbEpisodeGroupSeasonResource> Groups { get; set; } = new();
    }

    public class TmdbEpisodeGroupSeasonResource
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        [JsonProperty("episodes")]
        public List<TmdbEpisodeResource> Episodes { get; set; } = new();
    }
}
