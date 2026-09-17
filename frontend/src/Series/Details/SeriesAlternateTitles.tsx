import React from 'react';
import { AlternateTitle } from 'Series/Series';
import translate from 'Utilities/String/translate';
import styles from './SeriesAlternateTitles.css';

interface SeriesAlternateTitlesProps {
  alternateTitles: AlternateTitle[];
  aliases?: string[];
}

function SeriesAlternateTitles({
  alternateTitles,
  aliases = [],
}: SeriesAlternateTitlesProps) {
  return (
    <div>
      {aliases.length ? (
        <div className={styles.section}>
          <div className={styles.sectionTitle}>{translate('Manual')}</div>

          <ul className={styles.titles}>
            {aliases.map((alias) => {
              return (
                <li key={alias} className={styles.alternateTitle}>
                  {alias}
                </li>
              );
            })}
          </ul>
        </div>
      ) : null}

      {alternateTitles.length ? (
        <div className={styles.section}>
          <div className={styles.sectionTitle}>{translate('Automatic')}</div>

          <ul className={styles.titles}>
            {alternateTitles.map((alternateTitle) => {
              return (
                <li
                  key={alternateTitle.title}
                  className={styles.alternateTitle}
                >
                  {alternateTitle.title}

                  {alternateTitle.comment ? (
                    <span className={styles.comment}>
                      {' '}
                      {alternateTitle.comment}
                    </span>
                  ) : null}
                </li>
              );
            })}
          </ul>
        </div>
      ) : null}
    </div>
  );
}

export default SeriesAlternateTitles;
