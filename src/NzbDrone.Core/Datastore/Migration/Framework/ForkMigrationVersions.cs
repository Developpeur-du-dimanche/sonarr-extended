using System.Collections.Generic;

namespace NzbDrone.Core.Datastore.Migration.Framework
{
    // Fork migrations were first numbered right after upstream's and moved to 10000+ once upstream started using those numbers.
    // The description is matched as well so an upstream migration that has taken one of these versions is left alone.
    public static class ForkMigrationVersions
    {
        public static readonly IReadOnlyDictionary<long, string> ReleasedUpstreamVersions = new Dictionary<long, string>
        {
            { 234, nameof(add_series_episode_order) },
            { 235, nameof(add_series_aliases) },
            { 236, nameof(add_season_to_series_aliases) },
            { 237, nameof(add_series_search_by_absolute_number) }
        };
    }
}
