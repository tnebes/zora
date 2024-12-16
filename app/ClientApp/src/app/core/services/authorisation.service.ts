import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders, HttpErrorResponse} from '@angular/common/http';
import {Observable, BehaviorSubject, catchError, tap, of, map} from 'rxjs';
import {Constants} from '../constants';

@Injectable({
  providedIn: 'root'
})
export class AuthorisationService {
  private readonly adminStatusSubject: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);
  public readonly isAdmin$: Observable<boolean> = this.adminStatusSubject.asObservable();

  constructor(private readonly http: HttpClient) {
  }

  public checkAdminStatus(): Observable<boolean> {
    return this.http.get(Constants.IS_ADMIN, {
      headers: new HttpHeaders({
        'Content-Type': 'application/json'
      }),
      observe: 'response'
    }).pipe(
      map((response) => response.status === 200),
      tap((isAdmin: boolean) => {
        this.adminStatusSubject.next(isAdmin);
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('Admin status check failed:', error);
        this.adminStatusSubject.next(false);
        return of(false);
      })
    );
  }

  public refreshAdminStatus(): void {
    this.checkAdminStatus().subscribe();
  }
}
