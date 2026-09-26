using System.Collections.Generic;
using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Tv.Aliases
{
    public class SeriesAliasesUpdatedEvent : IEvent
    {
        public List<int> SeriesIds { get; private set; }

        public SeriesAliasesUpdatedEvent(List<int> seriesIds)
        {
            SeriesIds = seriesIds;
        }
    }
}
