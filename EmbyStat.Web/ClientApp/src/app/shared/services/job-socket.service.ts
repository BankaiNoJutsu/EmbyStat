import { Injectable } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { Job } from '../../jobs/models/job';

import * as moment from 'moment';

@Injectable()

export class JobSocketService {
  private lines: SafeHtml[];
  logsSubject = new BehaviorSubject<SafeHtml[]>([]);
  infoSubject = new BehaviorSubject<Job>(null);
  missedPingsSubject = new BehaviorSubject<number>(0);
  isSyncRunning = false;

  constructor(private sanitizer: DomSanitizer) {
    this.lines = [];
  }

  updateTasksLogs(value: string, type: number) {
    const now = moment().format('HH:mm:ss DD-MM-YY');
    const line = now + ' ' + value;

    if (type === 1) {
      this.lines.push(this.sanitizer.bypassSecurityTrustHtml('<span class="text__accent">' + line + '</span>'));
    } else if (type === 2) {
      this.lines.push(this.sanitizer.bypassSecurityTrustHtml('<span class="text__warn">' + line + '</span>'));
    } else {
      this.lines.push(line);
    }

    if (this.lines.length >= 20) {
      this.lines.shift();
    }

    this.logsSubject.next(this.lines);
  }

  updateTasksInfo(job: Job) {
    this.isSyncRunning = !!job && job.state === 2 && job.id === 'be68900b-ee1d-41ef-b12f-60ef3106052e';
    this.infoSubject.next(job);
  }

  updateMissedPings(count: number) {
    this.missedPingsSubject.next(count);
  }
}
