using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.CustomFormats
{
    public class MediaInfoLanguageSpecificationValidator : AbstractValidator<MediaInfoLanguageSpecificationBase>
    {
        public MediaInfoLanguageSpecificationValidator()
        {
            RuleFor(c => c.Value).Custom((value, context) =>
            {
                if (!Language.All.Any(o => o.Id == value))
                {
                    context.AddFailure(string.Format("Invalid Language condition value: {0}", value));
                }
            });
        }
    }

    public abstract class MediaInfoLanguageSpecificationBase : CustomFormatSpecificationBase
    {
        private static readonly MediaInfoLanguageSpecificationValidator Validator = new MediaInfoLanguageSpecificationValidator();

        [FieldDefinition(1, Label = "CustomFormatsSpecificationLanguage", Type = FieldType.Select, SelectOptions = typeof(LanguageFieldConverter))]
        public int Value { get; set; }

        [FieldDefinition(1, Label = "CustomFormatsSpecificationExceptLanguage", HelpText = "CustomFormatsSpecificationExceptLanguageHelpText", Type = FieldType.Checkbox)]
        public bool ExceptLanguage { get; set; }

        protected abstract IEnumerable<string> GetStreamLanguages(MediaInfoModel mediaInfo);

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
            var comparedLanguage = input.Series != null && Value == Language.Original.Id && input.Series.OriginalLanguage != Language.Unknown
                ? input.Series.OriginalLanguage
                : (Language)Value;

            var languages = GetStreamLanguages(input.MediaInfo)
                .Select(l => l.IsNotNullOrWhiteSpace() ? IsoLanguages.Find(l)?.Language ?? Language.Unknown : Language.Unknown)
                .Distinct()
                .ToList();

            if (ExceptLanguage)
            {
                return languages.Any(l => l != comparedLanguage);
            }

            return languages.Contains(comparedLanguage);
        }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
