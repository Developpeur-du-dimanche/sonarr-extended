using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats.Specifications.MediaInfoSpecification
{
    [TestFixture]
    public class SubtitleLanguageFixture : CoreTest<SubtitleLanguageSpecification>
    {
        private CustomFormatInput _input;

        [SetUp]
        public void Setup()
        {
            _input = new CustomFormatInput
            {
                MediaInfo = new MediaInfoModel
                {
                    AudioStreams = new List<MediaInfoAudioStreamModel>
                    {
                        new() { Language = "fre" }
                    },
                    SubtitleStreams = new List<MediaInfoSubtitleStreamModel>
                    {
                        new() { Language = "eng" },
                        new() { Language = "fre", Forced = true }
                    }
                }
            };
        }

        [Test]
        public void should_match_subtitle_language()
        {
            Subject.Value = Language.English.Id;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_audio_only_language()
        {
            _input.MediaInfo.SubtitleStreams.RemoveAt(1);
            Subject.Value = Language.French.Id;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_match_forced_subtitle_by_default()
        {
            Subject.Value = Language.French.Id;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_forced_subtitle_when_ignored()
        {
            Subject.Value = Language.French.Id;
            Subject.IgnoreForced = true;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_not_match_when_file_has_no_subtitles()
        {
            _input.MediaInfo.SubtitleStreams = null;
            Subject.Value = Language.English.Id;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }
    }
}
