import React, { useCallback } from 'react';
import TextInput from 'Components/Form/TextInput';
import IconButton from 'Components/Link/IconButton';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import { InputChanged } from 'typings/inputs';
import translate from 'Utilities/String/translate';
import styles from './SeriesAliasRow.css';

interface SeriesAliasRowProps {
  id: number;
  title: string;
  isDuplicate: boolean;
  onTitleChange: (id: number, title: string) => void;
  onRemovePress: (id: number) => void;
}

function SeriesAliasRow({
  id,
  title,
  isDuplicate,
  onTitleChange,
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
