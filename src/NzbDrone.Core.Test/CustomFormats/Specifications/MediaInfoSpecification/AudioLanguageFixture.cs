using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.CustomFormats.Specifications.MediaInfoSpecification
{
    [TestFixture]
    public class AudioLanguageFixture : CoreTest<AudioLanguageSpecification>
    {
        private CustomFormatInput _input;

        [SetUp]
        public void Setup()
        {
            _input = new CustomFormatInput
            {
                Series = Builder<Series>.CreateNew().With(s => s.OriginalLanguage = Language.Japanese).Build(),
                MediaInfo = new MediaInfoModel
                {
                    AudioStreams = new List<MediaInfoAudioStreamModel>
                    {
                        new() { Language = "jpn" },
                        new() { Language = "fre" }
                    }
                }
            };
        }

        [TestCase("fre")]
        [TestCase("fra")]
        [TestCase("fr")]
        public void should_match_language_from_iso_code(string isoCode)
        {
            _input.MediaInfo.AudioStreams[1].Language = isoCode;
            Subject.Value = Language.French.Id;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_missing_language()
        {
            Subject.Value = Language.English.Id;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_match_negated_missing_language()
        {
            Subject.Value = Language.English.Id;
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_match_original_language()
        {
            Subject.Value = Language.Original.Id;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_match_except_language_when_another_track_is_present()
        {
            Subject.Value = Language.French.Id;
            Subject.ExceptLanguage = true;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_match_unknown_for_track_without_language()
        {
            _input.MediaInfo.AudioStreams[1].Language = null;
            Subject.Value = Language.Unknown.Id;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [TestCase(false)]
        [TestCase(true)]
        public void should_not_match_without_media_info(bool negate)
        {
            _input.MediaInfo = null;
            Subject.Value = Language.French.Id;
            Subject.Negate = negate;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }
    }
}
