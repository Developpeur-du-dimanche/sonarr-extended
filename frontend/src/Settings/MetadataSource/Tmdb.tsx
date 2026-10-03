import React, { useCallback, useEffect } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import FormInput from 'Components/Form/FormInput';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import FormRow from 'Components/Form/FormRow';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import { inputTypes, kinds } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import {
  OnChildStateChange,
  SetChildSave,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import { useManageMetadataSourceSettings } from './useMetadataSourceSettings';

interface TmdbProps {
  setChildSave: SetChildSave;
  onChildStateChange: OnChildStateChange;
}

function Tmdb({ setChildSave, onChildStateChange }: TmdbProps) {
  const {
    isFetching,
    isFetched,
    isSaving,
    error,
    settings,
    hasSettings,
    hasPendingChanges,
    saveSettings,
    updateSetting,
  } = useManageMetadataSourceSettings();

  const handleInputChange = useCallback(
    (change: InputChanged) => {
      // @ts-expect-error input change events aren't typed
      updateSetting('tmdbApiKey', change.value);
    },
    [updateSetting]
  );

  useEffect(() => {
    setChildSave(saveSettings);
  }, [saveSettings, setChildSave]);

  useEffect(() => {
    onChildStateChange({
      isSaving,
      hasPendingChanges,
    });
  }, [hasPendingChanges, isSaving, onChildStateChange]);

  return (
    <FieldSet legend={translate('TheMovieDb')}>
      {isFetching && !isFetched ? <LoadingIndicator /> : null}

      {!isFetching && error ? (
        <Alert kind={kinds.DANGER}>
          {translate('MetadataSourceSettingsLoadError')}
        </Alert>
      ) : null}

      {hasSettings && isFetched && !error ? (
        <FormRow>
          <FormLabel>{translate('TmdbApiKey')}</FormLabel>
          <FormInputHelpText text={translate('TmdbApiKeyHelpText')} />
          <FormInput
            type={inputTypes.PASSWORD}
            name="tmdbApiKey"
            {...settings.tmdbApiKey}
            onChange={handleInputChange}
          />
        </FormRow>
      ) : null}
    </FieldSet>
  );
}

export default Tmdb;
