import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItemDescription from 'Components/DescriptionList/DescriptionListItemDescription';
import DescriptionListItemTitle from 'Components/DescriptionList/DescriptionListItemTitle';
import FieldSet from 'Components/FieldSet';
import Link from 'Components/Link/Link';
import translate from 'Utilities/String/translate';

function MoreInfo() {
  return (
    <FieldSet legend={translate('MoreInfo')}>
      <DescriptionList>
        <DescriptionListItemTitle>
          {translate('HomePage')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://github.com/Developpeur-du-dimanche/sonarr-extended">
            github.com/Developpeur-du-dimanche/sonarr-extended
          </Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>{translate('Wiki')}</DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://wiki.servarr.com/sonarr">
            wiki.servarr.com/sonarr
          </Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Donations')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://sonarr.tv/donate">sonarr.tv/donate</Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('Source')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://github.com/Developpeur-du-dimanche/sonarr-extended">
            github.com/Developpeur-du-dimanche/sonarr-extended
          </Link>
        </DescriptionListItemDescription>
        <DescriptionListItemDescription>
          <Link to="https://github.com/Sonarr/Sonarr/">
            github.com/Sonarr/Sonarr
          </Link>
        </DescriptionListItemDescription>

        <DescriptionListItemTitle>
          {translate('FeatureRequests')}
        </DescriptionListItemTitle>
        <DescriptionListItemDescription>
          <Link to="https://github.com/Developpeur-du-dimanche/sonarr-extended/issues">
            github.com/Developpeur-du-dimanche/sonarr-extended/issues
          </Link>
        </DescriptionListItemDescription>
      </DescriptionList>
    </FieldSet>
  );
}

export default MoreInfo;
