using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.MetadataSource.Tmdb;
using Sonarr.Http;

namespace Sonarr.Api.V5.Tmdb;

[V5ApiController("tmdb/episodegroup")]
public class TmdbEpisodeGroupController : Controller
{
    private readonly ITmdbProxy _tmdbProxy;

    public TmdbEpisodeGroupController(ITmdbProxy tmdbProxy)
    {
        _tmdbProxy = tmdbProxy;
    }

    [HttpGet]
    [Produces("application/json")]
    public Ok<List<TmdbEpisodeGroupResource>> GetEpisodeGroups([FromQuery] int tmdbId)
    {
        if (tmdbId <= 0 || !_tmdbProxy.IsConfigured)
        {
            return TypedResults.Ok(new List<TmdbEpisodeGroupResource>());
        }

        return TypedResults.Ok(_tmdbProxy.GetEpisodeGroups(tmdbId).ToResource());
    }
}
