import { Component, OnDestroy, Input, Output, EventEmitter } from '@angular/core';
import { Subscription } from 'rxjs';

import { EmbyUser } from '../../models/emby/emby-user';
import { SettingsFacade } from '../../../settings/state/facade.settings';
import { Settings } from '../../../settings/models/settings';
import { ConfigHelper } from '../../helpers/configHelper';

@Component({
  selector: 'app-card-user',
  templateUrl: './card-user.component.html',
  styleUrls: ['./card-user.component.scss']
})
export class CardUserComponent implements OnDestroy {
  private settingsSub: Subscription;
  private settings: Settings;

  @Input() user: EmbyUser;
  @Output() clicked = new EventEmitter<string>();

  constructor(private readonly settingsFacade: SettingsFacade) {
    this.settingsSub = settingsFacade.getSettings().subscribe(data => this.settings = data);
  }

  getEmbyAddress(): string {
    return ConfigHelper.getFullEmbyAddress(this.settings);
  }

  ngOnDestroy() {
    if (this.settingsSub !== undefined) {
      this.settingsSub.unsubscribe();
    }
  }

  openUser(): void {
    this.clicked.emit(this.user.id);
  }
}
