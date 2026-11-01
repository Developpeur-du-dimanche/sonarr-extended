using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.CustomFormats
{
    public class AudioLanguageSpecification : MediaInfoLanguageSpecificationBase
    {
        public override int Order => 11;
        public override string ImplementationName => "Audio Track Language";

        protected override IEnumerable<string> GetStreamLanguages(MediaInfoModel mediaInfo)
        {
            return mediaInfo.AudioStreams?.Select(s => s.Language) ?? [];
        }
    }
}
