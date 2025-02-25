import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError, switchMap } from 'rxjs/operators';
import { AuthorisationService } from '../services/authorisation.service';
import { AuthenticationService } from '../services/authentication.service';
@Injectable({
  providedIn: 'root'
})
export class AuthAndAdminGuard implements CanActivate {
  constructor(
    private readonly authorisationService: AuthorisationService,
    private readonly authenticationService: AuthenticationService,
    private readonly router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> {
    return this.checkAuthentication().pipe(
      switchMap(isAuthenticated => {
        if (!isAuthenticated) {
          return of(this.router.createUrlTree(['/login']));
        }
        return this.checkAdminStatus().pipe(
          map(isAdmin => isAdmin ? true : this.router.createUrlTree(['/unauthorized']))
        );
      }),
      catchError(() => of(this.router.createUrlTree(['/error'])))
    );
  }

  private checkAuthentication(): Observable<boolean> {
    return this.authenticationService.checkAuthStatus().pipe(
      map(result => result.isAuthenticated)
    );
  }

  private checkAdminStatus(): Observable<boolean> {
    return this.authorisationService.checkAdminStatus().pipe(
      map(isAdmin => {
        if (!isAdmin) {
          this.router.navigate(['/unauthorized']);
        }
        return isAdmin;
      }),
      catchError(() => {
        this.router.navigate(['/error']);
        return of(false);
      })
    );
  }
  private handleError(): Observable<UrlTree> {
    return of(this.router.createUrlTree(['/error']));
  }
}