import {Component, OnInit, OnDestroy} from '@angular/core';
import {Router} from '@angular/router';
import {Subject, takeUntil, Observable, map} from 'rxjs';
import {AuthenticationService} from '../core/services/authentication.service';
import {AuthorisationService} from '../core/services/authorisation.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit, OnDestroy {
  public isExpanded: boolean = false;
  public isAuthenticated$: Observable<boolean>;
  public isAdmin$: Observable<boolean>;
  public currentUserId$: Observable<string>;

  private readonly destroy$: Subject<void> = new Subject<void>();

  constructor(
    private readonly authenticationService: AuthenticationService,
    private readonly authorisationService: AuthorisationService,
    private readonly router: Router
  ) {
    this.isAuthenticated$ = this.authenticationService.authState$;
    this.isAdmin$ = this.authorisationService.isAdmin$;
    this.currentUserId$ = this.authenticationService.currentUser$.pipe(
      map((user) => user ? user.id : '')
    );
  }

  public ngOnInit(): void {
    this.authenticationService.authState$.pipe(
      takeUntil(this.destroy$)
    ).subscribe((isAuthenticated: boolean) => {
      if (isAuthenticated) {
        this.authorisationService.checkAdminStatus().subscribe();
      }
    });

    this.authenticationService.checkAuthStatus().pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      error: (error: Error) => {
        console.error('Auth check failed:', error);
      }
    });

    this.authenticationService.currentUser().pipe(
      takeUntil(this.destroy$)
    ).subscribe();
  }

  public ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public collapse(): void {
    this.isExpanded = false;
  }

  public toggle(): void {
    this.isExpanded = !this.isExpanded;
  }

  public logout(): void {
    this.authenticationService.logout();
    this.router.navigate(['/']);
  }
}
