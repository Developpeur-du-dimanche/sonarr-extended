namespace NzbDrone.Core.Tv.Aliases
{
    public class SeriesAliasWithTvdbId
    {
        public int TvdbId { get; set; }
        public string Title { get; set; }
        public int? SeasonNumber { get; set; }
        public int? SceneSeasonNumber { get; set; }
    }
}
