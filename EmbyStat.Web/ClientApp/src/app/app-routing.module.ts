import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AboutOverviewComponent } from './pages/about/about-overview/about-overview.component';
import {
    DashboardOverviewComponent
} from './pages/dashboard/dashboard-overview/dashboard-overview.component';
import { MovieContainerComponent } from './pages/movie/movie-container/movie-container.component';
import { MovieOverviewComponent } from './pages/movie/movie-overview/movie-overview.component';
import {
    SettingsContainerComponent
} from './pages/settings/settings-container/settings-container.component';
import {
    SettingsGeneralComponent
} from './pages/settings/settings-general/settings-general.component';

const routes: Routes = [
  { path: '', component: DashboardOverviewComponent },
  { path: 'about', component: AboutOverviewComponent },
  {
    path: 'movie', component: MovieContainerComponent, children: [
      { path: 'overview', component: MovieOverviewComponent }
    ]
  },
  {
    path: 'settings', component: SettingsContainerComponent, children: [
      { path: 'general', component: SettingsGeneralComponent }
    ]
  },
  { path: '**', redirectTo: '' }];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
