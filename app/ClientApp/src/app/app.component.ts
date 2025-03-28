import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from './core/services/authentication.service';
import {switchMap, tap, catchError} from 'rxjs/operators';
import {of} from 'rxjs';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
    title = 'app';
    
    constructor(private readonly authService: AuthenticationService) {
    }
    
    ngOnInit(): void {
        // Check if we have a token before attempting the auth check
        const token = this.authService.getToken();
        if (!token) {
            return;
        }

        this.authService.checkAuthStatus().pipe(
            tap(status => {
                if (!status.isAuthenticated) {
                    this.authService.logout();
                }
            }),
            switchMap(status => {
                if (status.isAuthenticated) {
                    return this.authService.currentUser();
                }
                return of(null);
            }),
            catchError(() => {
                this.authService.logout();
                return of(null);
            })
        ).subscribe();
    }
}
