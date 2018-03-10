import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { ConfigurationComponent } from './configuration/configuration.component';
import { WizardComponent } from './wizard/wizard.component';

const routes: Routes = [{ path: '', component: DashboardComponent },
  { path: 'configuration', component: ConfigurationComponent },
  { path: 'wizard', component: WizardComponent },
  { path: '**', redirectTo: '/' }];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
