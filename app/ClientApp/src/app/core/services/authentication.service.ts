import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs/internal/Observable';
import { Constants } from '../constants';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) {}

  login(username: string, password: string) : Observable<string> {
    return this.http.post<string>(`${Constants.API.TOKEN}`, { "username": username, "password": password });
  }

  saveToken(token: string) : void {
    localStorage.setItem(Constants.API.JWT_TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(Constants.API.JWT_TOKEN_KEY);
  }

  logout() : void {
    localStorage.removeItem(Constants.API.JWT_TOKEN_KEY);
  }
}
