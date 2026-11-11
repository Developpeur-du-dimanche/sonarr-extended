using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.CustomFormats.Specifications.MediaInfoSpecification
{
    [TestFixture]
    public class AudioTrackTitleFixture : CoreTest<AudioTrackTitleSpecification>
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
                        new() { Title = "English 5.1" },
                        new() { Title = null },
                        new() { Title = "Commentary with the director" }
                    },
                    SubtitleStreams = new List<MediaInfoSubtitleStreamModel>
                    {
                        new() { Title = "SDH" }
                    }
                }
            };
        }

        [TestCase(@"commentary")]
        [TestCase(@"\b5\.1\b")]
        public void should_match_any_audio_track_title(string pattern)
        {
            Subject.Value = pattern;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_subtitle_track_title()
        {
            Subject.Value = "SDH";

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_match_negated_missing_title()
        {
            Subject.Value = "Descriptive";
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void should_not_match_without_media_info(bool negate)
        {
            _input.MediaInfo = null;
            Subject.Value = "Commentary";
            Subject.Negate = negate;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }
    }

    [TestFixture]
    public class SubtitleTrackTitleFixture : CoreTest<SubtitleTrackTitleSpecification>
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
                        new() { Title = "Commentary" }
                    },
                    SubtitleStreams = new List<MediaInfoSubtitleStreamModel>
                    {
                        new() { Title = "Full" },
                        new() { Title = "Signs & Songs" }
                    }
                }
            };
        }

        [Test]
        public void should_match_subtitle_track_title()
        {
            Subject.Value = @"signs\W+songs";

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_audio_track_title()
        {
            Subject.Value = "Commentary";

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_not_match_when_file_has_no_subtitles()
        {
            _input.MediaInfo.SubtitleStreams = null;
            Subject.Value = "Full";

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }
    }
}
