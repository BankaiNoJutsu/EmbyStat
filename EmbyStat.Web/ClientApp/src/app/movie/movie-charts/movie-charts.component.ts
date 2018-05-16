import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';

import { MovieChartsService } from '../service/movie-charts.service';
import { MovieFacade } from '../state/facade.movie';
import { MovieGraphs } from '../models/movieGraphs';

@Component({
  selector: 'app-movie-charts',
  templateUrl: './movie-charts.component.html',
  styleUrls: ['./movie-charts.component.scss']
})
export class MovieChartsComponent implements OnInit, OnDestroy  {
  private _selectedCollections: string[];

  get selectedCollections(): string[] {
    return this._selectedCollections;
  }

  @Input()
  set selectedCollections(collection: string[]) {
    if (collection === undefined) {
      collection = [];
    }

    this._selectedCollections = collection;

    if (this.onTab) {
      this.graphs$ = this.movieFacade.getGraphs(this._selectedCollections);
    }
  }

  public graphs$: Observable<MovieGraphs>;
  private movieChartSub: Subscription;
  private onTab = false;

  constructor(private movieFacade: MovieFacade, private movieChartsService: MovieChartsService) {
    this.movieChartSub = movieChartsService.open.subscribe(value => {
      this.onTab = value;
      if (value && this.graphs$ === undefined) {
        this.graphs$ = this.movieFacade.getGraphs(this._selectedCollections);
      } else {
        this.movieFacade.clearGraphs();
        this.graphs$ = undefined;
      }
    });
  }

  ngOnInit() {

  }

  ngOnDestroy(): void {
    this.movieChartsService.changeOpened(false);

    if (this.movieChartSub !== undefined) {
      this.movieChartSub.unsubscribe();
    }
  }
}
