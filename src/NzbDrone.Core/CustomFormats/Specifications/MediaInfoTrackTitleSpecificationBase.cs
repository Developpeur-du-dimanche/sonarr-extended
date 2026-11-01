using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.CustomFormats
{
    public abstract class MediaInfoTrackTitleSpecificationBase : RegexSpecificationBase
    {
        protected abstract IEnumerable<string> GetStreamTitles(MediaInfoModel mediaInfo);

        // Releases have no media info: the tracks are unknown, so neither the condition nor its negation can match.
        public override bool IsSatisfiedBy(CustomFormatInput input)
        {
            if (input.MediaInfo == null)
            {
                return false;
            }

            return base.IsSatisfiedBy(input);
        }

        protected override bool IsSatisfiedByWithoutNegate(CustomFormatInput input)
        {
            return GetStreamTitles(input.MediaInfo).Any(MatchString);
        }
    }
}
