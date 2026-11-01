using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.CustomFormats
{
    public class SubtitleLanguageSpecification : MediaInfoLanguageSpecificationBase
    {
        public override int Order => 12;
        public override string ImplementationName => "Subtitle Track Language";

        [FieldDefinition(2, Label = "CustomFormatsSpecificationIgnoreForcedSubtitles", HelpText = "CustomFormatsSpecificationIgnoreForcedSubtitlesHelpText", Type = FieldType.Checkbox)]
        public bool IgnoreForced { get; set; }

        protected override IEnumerable<string> GetStreamLanguages(MediaInfoModel mediaInfo)
        {
            return mediaInfo.SubtitleStreams?
                .Where(s => !IgnoreForced || s.Forced != true)
                .Select(s => s.Language) ?? [];
        }
    }
}
