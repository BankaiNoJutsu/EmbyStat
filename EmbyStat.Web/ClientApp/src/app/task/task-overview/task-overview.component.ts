import { Component, OnInit, OnDestroy } from '@angular/core';
import { SafeHtml } from '@angular/platform-browser';
import { Subscription } from 'rxjs/Subscription';
import { Task } from '../models/task';
import { MatDialog } from '@angular/material';
import { TriggerDialogComponent } from '../trigger-dialog/trigger-dialog.component';
import 'rxjs/Rx';
import * as moment from 'moment';

import { TaskSignalService } from '../../shared/services/task-signal.service';
import { TaskService } from '../service/task.service';

@Component({
  selector: 'app-task-overview',
  templateUrl: './task-overview.component.html',
  styleUrls: ['./task-overview.component.scss']
})
export class TaskOverviewComponent implements OnInit, OnDestroy {
  private getTasksSub: Subscription;
  private taskInfoSignalSub: Subscription;
  private taskLogsSignalSub: Subscription;
  private fireTaskSub: Subscription;
  tasks: Task[];
  lines: SafeHtml[] = [];

  constructor(private taskService: TaskService,
    public dialog: MatDialog,
    private taskSignalService: TaskSignalService) {

    this.taskLogsSignalSub = taskSignalService.logsSubject.subscribe(logs => {
      this.lines = logs;
    });

    this.taskInfoSignalSub = taskSignalService.infoSubject.subscribe(tasks => {
      this.tasks = tasks;
    });
  }

  ngOnInit() {
    //this.getTasksSub = this.taskService.getTasks().subscribe(data => this.tasks = data);
  }

  firePing() {
    this.taskService.firePingTask().subscribe();
  }


  openDialog(task: Task): void {
    const dialogRef = this.dialog.open(TriggerDialogComponent, {
      width: '500px',
      data: { task: task }
    });

    dialogRef.afterClosed().subscribe((result: Task) => {
      //if (result !== null) {
      //  this.taskService.updateTriggers(result).subscribe(data => {
      //    this.tasks = data;
      //  });
      //} else {
      //  this.getTasksSub = this.taskService.getTasks().subscribe(data => this.tasks = data);
      //}
    });
  }

  hasHours(time: Date, to = moment.utc()): boolean {
    const from = moment.utc(time);
    to = this.convertToMoment(to);

    const milliseconds = to.diff(from);
    const duration = moment.duration(milliseconds);
    return Math.floor(duration.asHours()) > 0;
  }

  hasMinutes(time: Date, to = moment.utc()): boolean {
    const from = moment.utc(time);
    to = this.convertToMoment(to);

    const milliseconds = to.diff(from);
    const duration = moment.duration(milliseconds);
    return Math.floor(duration.asMinutes()) % 60 > 0;
  }

  hasSeconds(time: Date, to = moment.utc()): boolean {
    const from = moment.utc(time);
    to = this.convertToMoment(to);

    const milliseconds = to.diff(from);
    const duration = moment.duration(milliseconds);
    return (Math.floor(duration.asSeconds()) % 60 + 1) > 0;
  }

  needsAnd(task: Task): boolean {
    return (this.hasHours(task.lastExecutionResult.endTimeUtc) ||
      this.hasMinutes(task.lastExecutionResult.endTimeUtc)) &&
      this.hasSeconds(task.lastExecutionResult.endTimeUtc);
  }

  needsAndFor(task: Task): boolean {
    return (this.hasHours(task.lastExecutionResult.startTimeUtc, moment.utc(task.lastExecutionResult.endTimeUtc)) ||
      this.hasMinutes(task.lastExecutionResult.startTimeUtc, moment.utc(task.lastExecutionResult.endTimeUtc))) &&
      this.hasSeconds(task.lastExecutionResult.startTimeUtc, moment.utc(task.lastExecutionResult.endTimeUtc));
  }

  needsComma(task: Task): boolean {
    return this.hasHours(task.lastExecutionResult.endTimeUtc) && this.hasMinutes(task.lastExecutionResult.endTimeUtc);
  }

  needsCommaFor(task: Task): boolean {
    return this.hasHours(task.lastExecutionResult.startTimeUtc, moment.utc(task.lastExecutionResult.endTimeUtc))
      && this.hasMinutes(task.lastExecutionResult.startTimeUtc, moment.utc(task.lastExecutionResult.endTimeUtc));
  }

  hasNoTime(task: Task): boolean {
    return !this.hasHours(task.lastExecutionResult.endTimeUtc) &&
      !this.hasMinutes(task.lastExecutionResult.endTimeUtc) &&
      !this.hasSeconds(task.lastExecutionResult.endTimeUtc);
  }

  fireTask(id: string): void {
    //this.fireTaskSub = this.taskService.fireTask(id).subscribe();
  }

  private convertToMoment(value: any) {
    if (value instanceof moment) {
      return value;
    } else {
      return moment.utc(value);
    }
  }

  ngOnDestroy() {
    if (this.getTasksSub !== undefined) {
      this.getTasksSub.unsubscribe();
    }

    if (this.taskInfoSignalSub !== undefined) {
      this.taskInfoSignalSub.unsubscribe();
    }

    if (this.taskLogsSignalSub !== undefined) {
      this.taskLogsSignalSub.unsubscribe();
    }

    if (this.fireTaskSub !== undefined) {
      this.fireTaskSub.unsubscribe();
    }
  }
}
