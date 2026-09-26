export type TmdbEpisodeGroupType =
  | 'originalAirDate'
  | 'absolute'
  | 'dvd'
  | 'digital'
  | 'storyArc'
  | 'production'
  | 'tv';

interface TmdbEpisodeGroup {
  id: string;
  name: string;
  description: string;
  type: TmdbEpisodeGroupType;
  episodeCount: number;
  groupCount: number;
}

export default TmdbEpisodeGroup;
