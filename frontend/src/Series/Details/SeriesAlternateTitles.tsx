import React from 'react';
import formatSeason from 'Season/formatSeason';
import { AlternateTitle, SeriesAlias } from 'Series/Series';
import translate from 'Utilities/String/translate';
import styles from './SeriesAlternateTitles.css';

function getAliasSeasonInfo({ seasonNumber, sceneSeasonNumber }: SeriesAlias) {
  if (seasonNumber === null || seasonNumber === undefined) {
    return null;
  }

  const season = formatSeason(seasonNumber) ?? '';

  if (sceneSeasonNumber === null || sceneSeasonNumber === undefined) {
    return season;
  }

  return translate('AliasesSeasonReleasedAs', { season, sceneSeasonNumber });
}

interface SeriesAlternateTitlesProps {
  alternateTitles: AlternateTitle[];
  aliases?: SeriesAlias[];
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
              const seasonInfo = getAliasSeasonInfo(alias);

              return (
                <li
                  key={`${alias.title}-${alias.seasonNumber ?? ''}-${
                    alias.sceneSeasonNumber ?? ''
                  }`}
                  className={styles.alternateTitle}
                >
                  {alias.title}

                  {seasonInfo ? (
                    <span className={styles.comment}> {seasonInfo}</span>
                  ) : null}
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
