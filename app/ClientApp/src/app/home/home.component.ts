import {Component} from '@angular/core';
import {Observable} from 'rxjs';
import {map} from 'rxjs/operators';
import {AuthenticationService} from '../core/services/authentication.service';

@Component({
    selector: 'app-home',
    templateUrl: './home.component.html',
})
export class HomeComponent {
    public isAuthenticated$: Observable<boolean>;
    public currentUserId$: Observable<string>;
    public currentUsername$: Observable<string>;
    public greeting: string;

    constructor(private readonly authenticationService: AuthenticationService) {
        this.isAuthenticated$ = this.authenticationService.authState$;
        this.currentUserId$ = this.authenticationService.currentUser$.pipe(
            map((user) => user ? user.id : '')
        );
        this.currentUsername$ = this.authenticationService.currentUser$.pipe(
            map((user) => user ? user.username : '')
        );
        this.greeting = this.getGreeting();
    }

    private getGreeting(): string {
        const hour = new Date().getHours();
        if (hour < 12) return 'Good morning';
        if (hour < 18) return 'Good afternoon';
        return 'Good evening';
    }
}
