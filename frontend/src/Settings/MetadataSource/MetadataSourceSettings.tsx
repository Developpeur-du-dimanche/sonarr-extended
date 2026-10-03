import React, { useCallback, useRef, useState } from 'react';
import PageContentBody from 'Components/Page/PageContentBody';
import PageHeading from 'Components/Page/PageHeading';
import settingsStyles from 'Settings/Settings.module.css';
import SettingsPage from 'Settings/SettingsPage';
import {
  SaveCallback,
  SettingsStateChange,
} from 'typings/Settings/SettingsState';
import translate from 'Utilities/String/translate';
import TheTvdb from './TheTvdb';
import Tmdb from './Tmdb';

function MetadataSourceSettings() {
  const saveTmdb = useRef<() => void>();

  const [isSaving, setIsSaving] = useState(false);
  const [hasPendingChanges, setHasPendingChanges] = useState(false);

  const handleSetChildSave = useCallback((saveCallback: SaveCallback) => {
    saveTmdb.current = saveCallback;
  }, []);

  const handleChildStateChange = useCallback(
    ({ isSaving, hasPendingChanges }: SettingsStateChange) => {
      setIsSaving(isSaving);
      setHasPendingChanges(hasPendingChanges);
    },
    []
  );

  const handleSavePress = useCallback(() => {
    saveTmdb.current?.();
  }, []);

  return (
    <SettingsPage
      title={translate('MetadataSourceSettings')}
      isSaving={isSaving}
      hasPendingChanges={hasPendingChanges}
      onSavePress={handleSavePress}
    >
      <PageContentBody>
        <div className={settingsStyles.section}>
          <PageHeading
            scope={translate('Settings')}
            title={translate('MetadataSource')}
          />
          <TheTvdb />

          <Tmdb
            setChildSave={handleSetChildSave}
            onChildStateChange={handleChildStateChange}
          />
        </div>
      </PageContentBody>
    </SettingsPage>
  );
}

export default MetadataSourceSettings;
