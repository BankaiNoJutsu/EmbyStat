import { Card } from '../../shared/models/card';
import { TimespanCard } from '../../shared/models/timespanCard';

export class MovieStats {
  public movieCount: Card ;
  public genreCount: Card ;
  public collectionCount: Card ;
  public mostUsedContainer: Card ;
  public highestRatedMovie: Card ;
  public lowestRatedMovie: Card ;
  public longestMovie: Card ;
  public shortestMovie: Card ;
  public oldestPremieredMovie: Card ;
  public youngestPremieredMovie: Card ;
  public youngestAddedMovie: Card ;
  public mostFeaturedMovieActor: Card ;
  public mostFeaturedMovieDirector: Card ;
  public lastPlayedMovie: Card ;
  public totalPlayableTime: TimespanCard ;
}
