using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv.Aliases
{
    public class SeriesAlias : ModelBase
    {
        public int SeriesId { get; set; }
        public string Title { get; set; }
        public int? SeasonNumber { get; set; }
        public int? SceneSeasonNumber { get; set; }
    }
}
