import { Observable } from 'rxjs';

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { Language } from '../models/language';
import { Settings } from '../models/settings/settings';

@Injectable()
export class SettingsService {
  private readonly baseUrl = '/api/settings';
  private getLanguagesUrl = `${this.baseUrl}/languages`;

  constructor(private http: HttpClient) {

  }

  getSettings(): Observable<Settings> {
    return this.http.get<Settings>(this.baseUrl);
  }

  updateSettings(settings: Settings): Observable<Settings> {
    console.log(settings);
    return this.http.put<Settings>(this.baseUrl, settings);
  }

  getLanguages(): Observable<Language[]> {
    return this.http.get<Language[]>(this.getLanguagesUrl);
  }
}
