import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs/internal/Observable';
import {Constants} from '../constants';

export interface LoginRequest {
  username: string;
  password: string;
  token: string;
}

export interface LoginResponse {
  token: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private readonly http: HttpClient) {
  }

  public login(username: string, password: string, token: string): Observable<LoginResponse> {
    token = token || '';
    const loginRequest: LoginRequest = {username, password, token};
    const headers: HttpHeaders = new HttpHeaders({'Content-Type': 'application/json'});

    return this.http.post<LoginResponse>(
      `${Constants.API.TOKEN}`,
      loginRequest,
      {headers}
    );
  }

  public saveToken(token: string): void {
    localStorage.setItem(Constants.API.JWT_TOKEN_KEY, token);
  }

  public getToken(): string {
    return localStorage.getItem(Constants.API.JWT_TOKEN_KEY) ?? '';
  }

  public logout(): void {
    localStorage.removeItem(Constants.API.JWT_TOKEN_KEY);
  }
}
