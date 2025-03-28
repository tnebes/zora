import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders, HttpErrorResponse} from '@angular/common/http';
import {Observable, BehaviorSubject, catchError, tap, of, switchMap, map} from 'rxjs';
import {Constants} from '../constants';

export interface LoginRequest {
    username: string;
    password: string;
    token: string;
}

export interface LoginResponse {
    token: string;
}

export interface User {
    id: string;
    username: string;
    email: string;
}

@Injectable({
    providedIn: 'root'
})
export class AuthenticationService {
    private readonly authStateSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
    public readonly authState$: Observable<boolean> = this.authStateSubject.asObservable();
    private readonly currentUserSubject: BehaviorSubject<User | null> = new BehaviorSubject<User | null>(null);
    public readonly currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();

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
            tap((response: LoginResponse) => {
                this.saveToken(response.token);
                this.setAuthState(true);
            }),
            switchMap((response: LoginResponse) => this.currentUser().pipe(
                map(() => response)
            ))
        );
    }

    public checkAuthStatus(): Observable<{ isAuthenticated: boolean }> {
        const token: string = this.getToken();
        
        if (!token) {
            this.setAuthState(false);
            this.currentUserSubject.next(null);
            return of({ isAuthenticated: false });
        }
        
        return this.http.get<{ isAuthenticated: boolean }>(`${Constants.AUTHENTICATION_CHECK}`)
            .pipe(
                tap(() => this.setAuthState(true)),
                catchError((error: HttpErrorResponse) => {
                    console.error('Authentication check failed:', error);
                    this.setAuthState(false);
                    this.currentUserSubject.next(null);
                    
                    if (error.status === 401) {
                        this.logout();
                    }
                    
                    return of({isAuthenticated: false});
                })
            );
    }

    public currentUser(): Observable<User | null> {
        const token: string = this.getToken();
        
        if (!token) {
            this.currentUserSubject.next(null);
            return of(null);
        }
        
        return this.http.get<User>(`${Constants.CURRENT_USER}`)
            .pipe(
                tap((user: User) => this.currentUserSubject.next(user)),
                catchError((error: HttpErrorResponse) => {
                    console.error('Current user fetch failed:', error);
                    this.currentUserSubject.next(null);
                    
                    if (error.status === 401) {
                        this.logout();
                    }
                    
                    return of(null);
                })
            );
    }

    public saveToken(token: string): void {
        localStorage.setItem(Constants.JWT_TOKEN_KEY, token);
    }

    public getToken(): string {
        try {
            const storedToken: string | null = localStorage.getItem(Constants.JWT_TOKEN_KEY);
            return storedToken || '';
        } catch (error) {
            console.error('Error accessing localStorage:', error);
            return '';
        }
    }

    public logout(): void {
        localStorage.removeItem(Constants.JWT_TOKEN_KEY);
        this.setAuthState(false);
        this.currentUserSubject.next(null);
    }

    private setAuthState(isAuthenticated: boolean): void {
        this.authStateSubject.next(isAuthenticated);
    }

    public isAuthenticated(): boolean {
        return this.authStateSubject.getValue();
    }
}
