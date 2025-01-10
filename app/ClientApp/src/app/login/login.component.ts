import {Component} from '@angular/core';
import {AuthenticationService, LoginResponse} from '../core/services/authentication.service';
import {Router} from '@angular/router';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    username: string = '';
    password: string = '';

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly router: Router
    ) {
    }

    public onLogin(): void {
        const token: string = this.authenticationService.getToken();
        this.authenticationService.login(this.username, this.password, token)
            .subscribe({
                next: (response: LoginResponse) => {
                    this.authenticationService.saveToken(response.token);
                    this.router.navigate(['/'], {replaceUrl: true}).then(() => window.location.reload());
                },
                error: (error: Error) => {
                    console.error('Login failed:', error);
                }
            });
    }
}
