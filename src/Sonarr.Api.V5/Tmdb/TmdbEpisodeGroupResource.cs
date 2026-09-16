using NzbDrone.Core.MetadataSource.Tmdb;

namespace Sonarr.Api.V5.Tmdb;

public class TmdbEpisodeGroupResource
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public TmdbEpisodeGroupType Type { get; set; }
    public int EpisodeCount { get; set; }
    public int GroupCount { get; set; }
}

public static class TmdbEpisodeGroupResourceMapper
{
    public static TmdbEpisodeGroupResource ToResource(this TmdbEpisodeGroup model)
    {
        return new TmdbEpisodeGroupResource
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Type = model.Type,
            EpisodeCount = model.EpisodeCount,
            GroupCount = model.GroupCount
        };
    }

    public static List<TmdbEpisodeGroupResource> ToResource(this IEnumerable<TmdbEpisodeGroup> models)
    {
        return models.Select(ToResource).ToList();
    }
}
