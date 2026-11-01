using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.CustomFormats
{
    public class AudioTrackTitleSpecification : MediaInfoTrackTitleSpecificationBase
    {
        public override int Order => 13;
        public override string ImplementationName => "Audio Track Title";

        protected override IEnumerable<string> GetStreamTitles(MediaInfoModel mediaInfo)
        {
            return mediaInfo.AudioStreams?.Select(s => s.Title) ?? [];
        }
    }
}
