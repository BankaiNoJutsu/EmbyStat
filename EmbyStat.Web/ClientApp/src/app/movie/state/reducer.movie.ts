import { ApplicationState } from "../../states/app.state";

import { MovieStats } from '../models/movieStats';
import { MoviePersonStats } from '../models/moviePersonStats';
import { MovieStore } from '../models/movieStore';
import { MovieActions, MovieActionTypes } from './actions.movie';

const INITIAL_STATE: MovieStore = {
  stats: new MovieStats(),
  personStats: new MoviePersonStats(),
  collections: [],
  duplicates: []
};

export function MovieReducer(state: MovieStore = INITIAL_STATE, action: MovieActions) {
  switch (action.type) {
    case MovieActionTypes.LOAD_STATS_GENERAL_SUCCESS:
      return {
        ...state,
        stats: action.payload
      };
    case MovieActionTypes.LOAD_STATS_PERSON_SUCCESS:
      return {
        ...state,
        personStats: action.payload
      };
    case MovieActionTypes.LOAD_MOVIE_COLLECTIONS_SUCCESS:
      return {
        ...state,
        collections: action.payload
      };
    case MovieActionTypes.LOAD_DUPLICATE_GRAPH_SUCCESS:
      return {
        ...state,
        duplicates: action.payload
      };
    default:
      return state;
  }
}

export namespace MovieQuery {
  export const getGeneralStats = (state: ApplicationState) => state.movies.stats;
  export const getMovieCollections = (state: ApplicationState) => state.movies.collections;
  export const getPersonStats = (state: ApplicationState) => state.movies.personStats;
  export const getDuplicates = (state: ApplicationState) => state.movies.duplicates;
}
