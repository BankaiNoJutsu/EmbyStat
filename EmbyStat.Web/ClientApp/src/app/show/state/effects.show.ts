import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Actions, Effect } from '@ngrx/effects';
import { map, switchMap, catchError } from 'rxjs/operators';
import 'rxjs/add/observable/throw';

import { ShowService } from '../service/show.service';
import {
  ShowActionTypes,
  LoadShowCollectionsAction,
  LoadShowCollectionsSuccessAction,
  LoadGeneralStatsAction,
  LoadGeneralStatsSuccessAction
} from './actions.show';
import { Collection } from '../../shared/models/collection';
import { ShowStats } from '../models/showStats';

import { EffectError } from '../../states/app.actions';

@Injectable()
export class ShowEffects {
  constructor(
    private actions$: Actions,
    private showService: ShowService) {
  }

  @Effect()
  getShowCollections$ = this.actions$
    .ofType(ShowActionTypes.LOAD_COLLECTIONS)
    .pipe(
    map((data: LoadShowCollectionsAction) => data.payload),
    switchMap(_ => {
      return this.showService.getCollections();
    }),
    map((collections: Collection[]) => {
      return new LoadShowCollectionsSuccessAction(collections);
    }),
    catchError((err: any, caught: Observable<Object>) => Observable.throw(new EffectError(err)))
    );

  @Effect()
  getGeneralStats$ = this.actions$
    .ofType(ShowActionTypes.LOAD_STATS_GENERAL)
    .pipe(
    map((data: LoadGeneralStatsAction) => data.payload),
    switchMap((list: string[]) => {
      return this.showService.getGeneralStats(list);
    }),
    map((stats: ShowStats) => {
      return new LoadGeneralStatsSuccessAction(stats);
    }),
    catchError((err: any, caught: Observable<Object>) => Observable.throw(new EffectError(err)))
    );
}
