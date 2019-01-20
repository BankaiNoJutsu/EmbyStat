import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

import { Observable } from 'rxjs/Observable';

import { Configuration } from '../models/configuration';

@Injectable()
export class ConfigurationService {
  private readonly baseUrl = '/api/configuration';

  constructor(private http: HttpClient) {

  }

  getConfiguration(): Observable<Configuration> {
    return this.http.get<Configuration>(this.baseUrl);
  }

  updateConfgiguration(configuration: Configuration): Observable<Configuration> {
    return this.http.put<Configuration>(this.baseUrl, configuration);
  }
}
