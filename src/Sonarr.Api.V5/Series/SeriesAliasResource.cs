using NzbDrone.Core.Tv.Aliases;

namespace Sonarr.Api.V5.Series;

public class SeriesAliasResource
{
    public string? Title { get; set; }
    public int? SeasonNumber { get; set; }
    public int? SceneSeasonNumber { get; set; }
}

public static class SeriesAliasResourceMapper
{
    public static SeriesAliasResource ToResource(this SeriesAlias model)
    {
        return new SeriesAliasResource
        {
            Title = model.Title,
            SeasonNumber = model.SeasonNumber,
            SceneSeasonNumber = model.SceneSeasonNumber
        };
    }

    public static SeriesAlias ToModel(this SeriesAliasResource resource)
    {
        return new SeriesAlias
        {
            Title = resource.Title,
            SeasonNumber = resource.SeasonNumber,
            SceneSeasonNumber = resource.SceneSeasonNumber
        };
    }
}
