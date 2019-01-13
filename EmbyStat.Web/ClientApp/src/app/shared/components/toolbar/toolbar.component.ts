import { Component, OnInit, OnDestroy, Output, EventEmitter } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { ConfigHelper } from '../../helpers/configHelper';

import { ConfigurationFacade } from '../../../configuration/state/facade.configuration';
import { Configuration } from '../../../configuration/models/configuration';
import { EmbyStatus } from '../../models/emby/emby-status';
import { JobSocketService } from '../../services/job-socket.service';
import { EmbyService } from '../../services/emby.service';
import { Job } from '../../../jobs/models/job';

@Component({
  selector: 'app-toolbar',
  templateUrl: './toolbar.component.html',
  styleUrls: ['./toolbar.component.scss']
})
export class ToolbarComponent implements OnInit, OnDestroy {
  configuration$: Observable<Configuration>;
  private embyStatusSeb: Subscription;
  private jobSocketSub: Subscription;
  private missedPingsSub: Subscription;
  runningJob: Job;

  missedPings: number;

  @Output()
  toggleSideNav = new EventEmitter<void>();

  constructor(
    private configurationFacade: ConfigurationFacade,
    private jobSocketService: JobSocketService,
    private embyService: EmbyService) {
    this.configuration$ = configurationFacade.configuration$;

    this.missedPings = 0;
    this.jobSocketSub = jobSocketService.infoSubject.subscribe((job: Job) => {
      if (job != null && job.state === 1) {
        this.runningJob = job;
      } else {
        this.runningJob = undefined;
      }
    });

    this.missedPingsSub = jobSocketService.missedPingsSubject.subscribe((count: number) => {
      this.missedPings = count;
    });
  }

  getFullAddress(config: Configuration): string {
    return ConfigHelper.getFullEmbyAddress(config);
  }

  ngOnInit() {

  }

  ngOnDestroy() {
    if (this.jobSocketSub !== undefined) {
      this.jobSocketSub.unsubscribe();
    }

    if (this.embyStatusSeb !== undefined) {
      this.embyStatusSeb.unsubscribe();
    }

    if (this.missedPingsSub !== undefined) {
      this.missedPingsSub.unsubscribe();
    }
  }
}
