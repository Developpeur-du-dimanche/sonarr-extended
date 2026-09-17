import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import Alert from 'Components/Alert';
import Form from 'Components/Form/Form';
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
import { useSaveSeries, useSingleSeries } from 'Series/useSeries';
import { getValidationFailures } from 'Utilities/selectSettings';
import filterAlternateTitles from 'Utilities/Series/filterAlternateTitles';
import translate from 'Utilities/String/translate';
import SeriesAliasRow from './SeriesAliasRow';
import SeriesAlternateTitleRow from './SeriesAlternateTitleRow';
import styles from './SeriesAliasesModalContent.css';

const SCENE_MAPPING_URL =
  'https://docs.google.com/spreadsheets/d/1PiIvzijwcdALKQWfGE3j4lwnOqmDkhB48fyQTArJpI4/edit?pli=1&gid=675284162#gid=675284162';

const COLUMNS: Column[] = [
  {
    name: 'title',
    label: () => translate('Alias'),
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
    (aliases ?? []).map((alias) => ({ id: nextId.current++, title: alias }))
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

  // Titles already recognized for this series, so a manual alias cannot repeat one.
  // This is a plain case-insensitive match; the server also rejects them using the
  // same normalization it applies when parsing releases.
  const knownKeys = useMemo(() => {
    const keys = new Set<string>();

    keys.add(title.trim().toLowerCase());
    providerTitles.forEach((t) => keys.add(t.title.trim().toLowerCase()));

    return keys;
  }, [title, providerTitles]);

  const duplicateIds = useMemo(() => {
    const seen = new Map<string, number>();
    const duplicates = new Set<number>();

    items.forEach(({ id, title: itemTitle }) => {
      const key = itemTitle.trim().toLowerCase();

      if (!key) {
        return;
      }

      if (knownKeys.has(key)) {
        duplicates.add(id);
      }

      const existingId = seen.get(key);

      if (existingId === undefined) {
        seen.set(key, id);
      } else {
        duplicates.add(existingId);
        duplicates.add(id);
      }
    });

    return duplicates;
  }, [items, knownKeys]);

  const handleTitleChange = useCallback((id: number, newTitle: string) => {
    setItems((currentItems) =>
      currentItems.map((item) =>
        item.id === id ? { ...item, title: newTitle } : item
      )
    );
  }, []);

  const handleRemovePress = useCallback((id: number) => {
    setItems((currentItems) => currentItems.filter((item) => item.id !== id));
  }, []);

  const handleAddPress = useCallback(() => {
    setItems((currentItems) => [
      ...currentItems,
      { id: nextId.current++, title: '' },
    ]);
  }, []);

  const handleSavePress = useCallback(() => {
    const seen = new Set<string>();
    const newAliases: string[] = [];

    items.forEach(({ title: itemTitle }) => {
      const trimmed = itemTitle.trim();
      const key = trimmed.toLowerCase();

      if (!trimmed || seen.has(key)) {
        return;
      }

      seen.add(key);
      newAliases.push(trimmed);
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
                {items.map(({ id, title: itemTitle }) => {
                  return (
                    <SeriesAliasRow
                      key={id}
                      id={id}
                      title={itemTitle}
                      isDuplicate={duplicateIds.has(id)}
                      onTitleChange={handleTitleChange}
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
