import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders, HttpErrorResponse} from '@angular/common/http';
import {Observable, BehaviorSubject, catchError, tap, of} from 'rxjs';
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
export class AuthenticationService {

  private readonly authStateSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public readonly authState$: Observable<boolean> = this.authStateSubject.asObservable();
  public readonly isAdmin$: Observable<boolean> = this.isAdmin();

  constructor(private readonly http: HttpClient) {
  }

  public login(username: string, password: string, token: string): Observable<LoginResponse> {
    token = token || '';
    const loginRequest: LoginRequest = {username, password, token};
    const headers: HttpHeaders = new HttpHeaders({'Content-Type': 'application/json'});

    return this.http.post<LoginResponse>(
      `${Constants.TOKEN}`,
      loginRequest,
      {headers}
    ).pipe(
      tap(() => this.setAuthState(true))
    );
  }

  public checkAuthStatus(): Observable<{ isAuthenticated: boolean }> {
    return this.http.get<{ isAuthenticated: boolean }>(`${Constants.AUTHENTICATION_CHECK}`)
      .pipe(
        tap(() => this.setAuthState(true)),
        catchError((error: HttpErrorResponse) => {
          console.error('Authentication check failed:', error)
          this.setAuthState(false);
          return of({isAuthenticated: false});
        })
      );
  }

  public isAdmin(): Observable<boolean> {
    return of(true);
  }

  public saveToken(token: string): void {
    localStorage.setItem(Constants.JWT_TOKEN_KEY, token);
  }

  public getToken(): string {
    return localStorage.getItem(Constants.JWT_TOKEN_KEY) ?? '';
  }

  public logout(): void {
    localStorage.removeItem(Constants.JWT_TOKEN_KEY);
    this.setAuthState(false);
  }

  private setAuthState(isAuthenticated: boolean): void {
    this.authStateSubject.next(isAuthenticated);
  }
}
