using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.CustomFormats
{
    public class SubtitleTrackTitleSpecification : MediaInfoTrackTitleSpecificationBase
    {
        public override int Order => 14;
        public override string ImplementationName => "Subtitle Track Title";

        protected override IEnumerable<string> GetStreamTitles(MediaInfoModel mediaInfo)
        {
            return mediaInfo.SubtitleStreams?.Select(s => s.Title) ?? [];
        }
    }
}
