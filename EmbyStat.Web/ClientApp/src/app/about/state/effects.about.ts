import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Actions, Effect } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { map, switchMap, catchError, withLatestFrom } from 'rxjs/operators';
import { of } from 'rxjs/observable/of';

import 'rxjs/add/observable/throw';

import { About } from '../models/about';
import { AboutService } from '../service/about.service';

import {
  LoadAboutAction, LoadAboutSuccessAction,
  NoNeedAboutAction, AboutActionTypes
} from './actions.about';

import { AboutQuery } from './reducer.about';
import { EffectError } from '../../states/app.actions';
import { ApplicationState } from '../../states/app.state';

@Injectable()
export class AboutEffects {
  constructor(
    private actions$: Actions,
    private aboutService: AboutService,
    private store: Store<ApplicationState>) {
  }

  public loaded$ = this.store.select(AboutQuery.getLoaded);

  @Effect()
  getConfiguration$ = this.actions$
    .ofType(AboutActionTypes.LOAD_ABOUT)
    .pipe(
      map((data: LoadAboutAction) => data.payload),
      withLatestFrom(this.loaded$),
      switchMap(([_, loaded]) => {
        return loaded
          ? of(null)
          : this.aboutService.getAbout();
      }),
      map((about: About | null) => {
        return about
          ? new LoadAboutSuccessAction(about)
          : new NoNeedAboutAction();
      }),
      catchError((err: any, caught: Observable<Object>) => Observable.throw(new EffectError(err)))
  );
}
