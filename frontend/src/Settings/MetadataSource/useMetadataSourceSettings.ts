import { useManageSettings } from 'Settings/useSettings';

export interface MetadataSourceSettingsModel {
  tmdbApiKey: string;
}

const PATH = '/settings/metadatasource';

export const useManageMetadataSourceSettings = () => {
  return useManageSettings<MetadataSourceSettingsModel>(PATH);
};
