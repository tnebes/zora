import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree, Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AuthorisationService } from '../services/authorisation.service';

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(
    private readonly authorisationService: AuthorisationService,
    private readonly router: Router
  ) {}

  canActivate(
    route: ActivatedRouteSnapshot,
    state: RouterStateSnapshot
  ): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.checkAdminStatus();
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
} 