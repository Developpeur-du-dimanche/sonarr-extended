import React from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import translate from 'Utilities/String/translate';
import styles from './SeriesAlternateTitleRow.css';

interface SeriesAlternateTitleRowProps {
  title: string;
  comment?: string;
}

function SeriesAlternateTitleRow({
  title,
  comment,
}: SeriesAlternateTitleRowProps) {
  return (
    <TableRow>
      <TableRowCell className={styles.title}>
        {title}

        {comment ? <span className={styles.comment}> {comment}</span> : null}
      </TableRowCell>

      <TableRowCell className={styles.source}>
        {translate('Automatic')}
      </TableRowCell>

      <TableRowCell className={styles.actions} />
    </TableRow>
  );
}

export default SeriesAlternateTitleRow;
