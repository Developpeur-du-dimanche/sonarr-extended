import React, { useCallback } from 'react';
import NumberInput, { NumberInputChanged } from 'Components/Form/NumberInput';
import SelectInput, { SelectInputOption } from 'Components/Form/SelectInput';
import TextInput from 'Components/Form/TextInput';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './SeriesAliasRow.module.css';

export const ALL_SEASONS = -1;

interface SeriesAliasRowProps {
  id: number;
  title: string;
  seasonNumber: number | null;
  releaseSeasonNumber: number | null;
  seasonOptions: SelectInputOption[];
  isDuplicate: boolean;
  onTitleChange: (id: number, title: string) => void;
  onSeasonChange: (id: number, seasonNumber: number | null) => void;
  onReleaseSeasonChange: (
    id: number,
    releaseSeasonNumber: number | null
  ) => void;
  onRemovePress: (id: number) => void;
}

function SeriesAliasRow({
  id,
  title,
  seasonNumber,
  releaseSeasonNumber,
  seasonOptions,
  isDuplicate,
  onTitleChange,
  onSeasonChange,
  onReleaseSeasonChange,
  onRemovePress,
}: SeriesAliasRowProps) {
  const handleTitleChange = useCallback(
    ({ value }: InputChanged<string>) => {
      onTitleChange(id, value);
    },
    [id, onTitleChange]
  );

  const handleTitleBlur = useCallback(() => {
    const trimmed = title.trim();

    if (trimmed !== title) {
      onTitleChange(id, trimmed);
    }
  }, [id, title, onTitleChange]);

  const handleSeasonChange = useCallback(
    ({ value }: InputChanged<string>) => {
      const newSeasonNumber = Number(value);

      onSeasonChange(
        id,
        newSeasonNumber === ALL_SEASONS ? null : newSeasonNumber
      );
    },
    [id, onSeasonChange]
  );

  const handleReleaseSeasonChange = useCallback(
    ({ value }: NumberInputChanged) => {
      onReleaseSeasonChange(id, value);
    },
    [id, onReleaseSeasonChange]
  );

  const handleRemovePress = useCallback(() => {
    onRemovePress(id);
  }, [id, onRemovePress]);

  return (
    <TableRow>
      <TableRowCell>
        <TextInput
          name={`alias-${id}`}
          value={title}
          hasError={isDuplicate}
          placeholder={translate('Alias')}
          onChange={handleTitleChange}
          onBlur={handleTitleBlur}
        />
      </TableRowCell>

      <TableRowCell className={styles.season}>
        <SelectInput
          name={`alias-season-${id}`}
          value={seasonNumber ?? ALL_SEASONS}
          values={seasonOptions}
          onChange={handleSeasonChange}
        />
      </TableRowCell>

      <TableRowCell className={styles.releaseSeason}>
        {seasonNumber === null ? null : (
          <NumberInput
            name={`alias-release-season-${id}`}
            value={releaseSeasonNumber}
            min={0}
            onChange={handleReleaseSeasonChange}
          />
        )}
      </TableRowCell>

      <TableRowCell className={styles.source}>
        {translate('Manual')}
      </TableRowCell>

      <TableRowCell className={styles.actions}>
        <IconButton
          title={translate('RemoveAlias')}
          aria-label={translate('RemoveAlias')}
          name={icons.REMOVE}
          onPress={handleRemovePress}
        />
      </TableRowCell>
    </TableRow>
  );
}

export default SeriesAliasRow;
