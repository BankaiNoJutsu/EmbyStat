import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { SharedModule } from '../shared/shared.module';

import { AboutOverviewComponent } from './about-overview/about-overview.component';
import { AboutService } from './service/about.service';
import { AboutFacade } from './state/facade.about';

@NgModule({
  imports: [
    CommonModule,
    TranslateModule,
    SharedModule
  ],
  providers: [
    AboutService,
    AboutFacade
  ],
  declarations: [AboutOverviewComponent]
})
export class AboutModule { }
