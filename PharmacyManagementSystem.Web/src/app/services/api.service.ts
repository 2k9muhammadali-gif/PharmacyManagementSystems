import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly apiUrl = environment.apiUrl;

  constructor(private http: HttpClient, private auth: AuthService) {}

  private getHeaders(): Record<string, string> {
    const token = this.auth.getToken();
    return token ? { Authorization: `Bearer ${token}` } : ({} as Record<string, string>);
  }

  get<T>(path: string, params?: Record<string, string | number>): Observable<T> {
    let httpParams = new HttpParams();
    if (params) {
      Object.entries(params).forEach(([k, v]) => {
        httpParams = httpParams.set(k, String(v));
      });
    }
    return this.http.get<T>(`${this.apiUrl}${path}`, {
      headers: this.getHeaders(),
      params: httpParams
    });
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<T>(`${this.apiUrl}${path}`, body, {
      headers: this.getHeaders()
    });
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http.put<T>(`${this.apiUrl}${path}`, body, {
      headers: this.getHeaders()
    });
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<T>(`${this.apiUrl}${path}`, {
      headers: this.getHeaders()
    });
  }
}
