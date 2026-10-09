import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
import { SelectInputOption } from 'Components/Form/SelectInput';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import Column from 'Components/Table/Column';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import usePrevious from 'Helpers/Hooks/usePrevious';
import { kinds } from 'Helpers/Props';
import formatSeason from 'Season/formatSeason';
import { SeriesAlias } from 'Series/Series';
import { useSaveSeries, useSingleSeries } from 'Series/useSeries';
import { getValidationFailures } from 'Utilities/selectSettings';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import translate from 'Utilities/String/translate';
import SeriesAliasRow, { ALL_SEASONS } from './SeriesAliasRow';
import SeriesAlternateTitleRow from './SeriesAlternateTitleRow';
import styles from './SeriesAliasesModalContent.module.css';

const SCENE_MAPPING_URL =
  'https://docs.google.com/spreadsheets/d/1PiIvzijwcdALKQWfGE3j4lwnOqmDkhB48fyQTArJpI4/edit?pli=1&gid=675284162#gid=675284162';

const COLUMNS: Column[] = [
  {
    name: 'title',
    label: () => translate('Alias'),
    isVisible: true,
  },
  {
    name: 'seasonNumber',
    label: () => translate('Season'),
    isVisible: true,
  },
  {
    name: 'releaseSeasonNumber',
    label: () => translate('AliasesReleaseSeason'),
    isVisible: true,
  },
  {
    name: 'source',
    label: () => translate('Source'),
    isVisible: true,
  },
  {
    name: 'actions',
    label: '',
    isVisible: true,
  },
];

interface AliasItem {
  id: number;
  title: string;
  seasonNumber: number | null;
  releaseSeasonNumber: number | null;
}

// Like the server, the release season is only kept when it differs from the season.
function getSceneSeasonNumber({
  seasonNumber,
  releaseSeasonNumber,
}: AliasItem) {
  return seasonNumber !== null &&
    releaseSeasonNumber !== null &&
    releaseSeasonNumber !== seasonNumber
    ? releaseSeasonNumber
    : null;
}

function getAliasKey(item: AliasItem) {
  return [
    item.title.trim().toLowerCase(),
    item.seasonNumber ?? '',
    getSceneSeasonNumber(item) ?? '',
  ].join('|');
}

export interface SeriesAliasesModalContentProps {
  seriesId: number;
  onModalClose: () => void;
}

function SeriesAliasesModalContent({
  seriesId,
  onModalClose,
}: SeriesAliasesModalContentProps) {
  const series = useSingleSeries(seriesId)!;
  const { title, aliases } = series;

  const nextId = useRef(0);

  const [items, setItems] = useState<AliasItem[]>(() =>
    (aliases ?? []).map((alias) => ({
      id: nextId.current++,
      title: alias.title,
      seasonNumber: alias.seasonNumber ?? null,
      releaseSeasonNumber:
        alias.sceneSeasonNumber ?? alias.seasonNumber ?? null,
    }))
  );

  const seasonOptions = useMemo<SelectInputOption[]>(
    () => [
      { key: ALL_SEASONS, value: translate('AllSeasons') },
      ...series.seasons.map(({ seasonNumber }) => ({
        key: seasonNumber,
        value: formatSeason(seasonNumber) ?? '',
      })),
    ],
    [series.seasons]
  );

  const providerTitles = useMemo(
    () =>
      filterAlternateTitles(
        series.alternateTitles,
        series.title,
        series.useSceneNumbering
      ),
    [series]
  );

  const { saveSeries, isSaving, saveError } = useSaveSeries();
  const wasSaving = usePrevious(isSaving);

  const { errors, warnings } = useMemo(
    () => getValidationFailures(saveError),
    [saveError]
  );

  // Titles already recognized for this series, so a manual alias cannot repeat one unless it
  // remaps the release season. This is a plain case-insensitive match; the server also rejects
  // them using the same normalization it applies when parsing releases.
  const providerTitleKeys = useMemo(
    () => new Set(providerTitles.map((t) => t.title.trim().toLowerCase())),
    [providerTitles]
  );

  const duplicateIds = useMemo(() => {
    const seriesTitleKey = title.trim().toLowerCase();
    const seen = new Map<string, number>();
    const duplicates = new Set<number>();

    items.forEach((item) => {
      const titleKey = item.title.trim().toLowerCase();

      if (!titleKey) {
        return;
      }

      if (
        titleKey === seriesTitleKey ||
        (providerTitleKeys.has(titleKey) && getSceneSeasonNumber(item) === null)
      ) {
        duplicates.add(item.id);
      }

      const key = getAliasKey(item);
      const existingId = seen.get(key);

      if (existingId === undefined) {
        seen.set(key, item.id);
      } else {
        duplicates.add(existingId);
        duplicates.add(item.id);
      }
    });

    return duplicates;
  }, [title, items, providerTitleKeys]);

  const updateItem = useCallback(
    (id: number, changes: Partial<Omit<AliasItem, 'id'>>) => {
      setItems((currentItems) =>
        currentItems.map((item) =>
          item.id === id ? { ...item, ...changes } : item
        )
      );
    },
    []
  );

  const handleTitleChange = useCallback(
    (id: number, newTitle: string) => {
      updateItem(id, { title: newTitle });
    },
    [updateItem]
  );

  const handleSeasonChange = useCallback(
    (id: number, seasonNumber: number | null) => {
      updateItem(id, { seasonNumber, releaseSeasonNumber: seasonNumber });
    },
    [updateItem]
  );

  const handleReleaseSeasonChange = useCallback(
    (id: number, releaseSeasonNumber: number | null) => {
      updateItem(id, { releaseSeasonNumber });
    },
    [updateItem]
  );

  const handleRemovePress = useCallback((id: number) => {
    setItems((currentItems) => currentItems.filter((item) => item.id !== id));
  }, []);

  const handleAddPress = useCallback(() => {
    setItems((currentItems) => [
      ...currentItems,
      {
        id: nextId.current++,
        title: '',
        seasonNumber: null,
        releaseSeasonNumber: null,
      },
    ]);
  }, []);

  const handleSavePress = useCallback(() => {
    const seen = new Set<string>();
    const newAliases: SeriesAlias[] = [];

    items.forEach((item) => {
      const trimmed = item.title.trim();
      const key = getAliasKey(item);

      if (!trimmed || seen.has(key)) {
        return;
      }

      seen.add(key);
      newAliases.push({
        title: trimmed,
        seasonNumber: item.seasonNumber,
        sceneSeasonNumber: getSceneSeasonNumber(item),
      });
    });

    saveSeries({
      ...series,
      aliases: newAliases,
    });
  }, [series, items, saveSeries]);

  useEffect(() => {
    if (!isSaving && wasSaving && !saveError) {
      onModalClose();
    }
  }, [isSaving, wasSaving, saveError, onModalClose]);

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>{translate('AliasesModalHeader', { title })}</ModalHeader>

      <ModalBody>
        <Form validationErrors={errors} validationWarnings={warnings}>
          <Alert kind={kinds.INFO}>
            <div>{translate('AliasesHelpText')}</div>

            <div className={styles.sceneMappingInfo}>
              {translate('AliasesSeasonHelpText')}
            </div>

            <div className={styles.sceneMappingInfo}>
              <InlineMarkdown
                data={translate('AliasesSceneMappingInfo', {
                  url: SCENE_MAPPING_URL,
                })}
              />
            </div>
          </Alert>

          {items.length || providerTitles.length ? (
            <Table columns={COLUMNS} horizontalScroll={false}>
              <TableBody>
                {items.map((item) => {
                  return (
                    <SeriesAliasRow
                      key={item.id}
                      id={item.id}
                      title={item.title}
                      seasonNumber={item.seasonNumber}
                      releaseSeasonNumber={item.releaseSeasonNumber}
                      seasonOptions={seasonOptions}
                      isDuplicate={duplicateIds.has(item.id)}
                      onTitleChange={handleTitleChange}
                      onSeasonChange={handleSeasonChange}
                      onReleaseSeasonChange={handleReleaseSeasonChange}
                      onRemovePress={handleRemovePress}
                    />
                  );
                })}

                {providerTitles.map((alternateTitle) => {
                  return (
                    <SeriesAlternateTitleRow
                      key={alternateTitle.title}
                      title={alternateTitle.title}
                      comment={alternateTitle.comment}
                    />
                  );
                })}
              </TableBody>
            </Table>
          ) : (
            <div className={styles.noAliases}>{translate('NoAliases')}</div>
          )}

          <div className={styles.addAlias}>
            <Button onPress={handleAddPress}>{translate('AddAlias')}</Button>
          </div>
        </Form>
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>{translate('Cancel')}</Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={handleSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

export default SeriesAliasesModalContent;
