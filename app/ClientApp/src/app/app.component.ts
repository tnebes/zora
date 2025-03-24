import {Component, OnInit} from '@angular/core';
import {AuthenticationService} from './core/services/authentication.service';
import {switchMap, tap} from 'rxjs/operators';
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
        this.authService.checkAuthStatus().pipe(
            switchMap(status => {
                if (status.isAuthenticated) {
                    return this.authService.currentUser();
                }
                return of(null);
            })
        ).subscribe();
    }
}
