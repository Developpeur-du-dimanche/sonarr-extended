namespace NzbDrone.Core.MetadataSource.Tmdb
{
    public class TmdbEpisodeGroup
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TmdbEpisodeGroupType Type { get; set; }
        public int EpisodeCount { get; set; }
        public int GroupCount { get; set; }
    }

    // https://developer.themoviedb.org/reference/tv-series-episode-groups
    public enum TmdbEpisodeGroupType
    {
        OriginalAirDate = 1,
        Absolute = 2,
        Dvd = 3,
        Digital = 4,
        StoryArc = 5,
        Production = 6,
        Tv = 7
    }
}
