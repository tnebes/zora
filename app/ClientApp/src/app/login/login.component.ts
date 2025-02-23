import {Component, OnInit} from '@angular/core';
import {AuthenticationService, LoginResponse} from '../core/services/authentication.service';
import {Router} from '@angular/router';
import {environment} from '../../environments/environment';
import {HttpErrorResponse} from '@angular/common/http';

@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.scss']
})
export class LoginComponent {
    username: string = '';
    password: string = '';
    errorMessage: string | null = null;

    constructor(
        private readonly authenticationService: AuthenticationService,
        private readonly router: Router
    ) {
    }

    private getErrorMessage(error: HttpErrorResponse): string {

        let errorMessage: string = '';

        if (error.status === 400 && error.error?.message?.includes('already authenticated')) {
            errorMessage += `\n${error.error.message}`;
        }

        if (environment.production) {
            return errorMessage + '\nInvalid username or password';
        }

        if (error.status === 400 && error.error?.message) {
            errorMessage += `\n${error.error.message}`;
        }

        return errorMessage || 'An error occurred';
    }

    public onLogin(): void {
        this.errorMessage = null;
        const token: string = this.authenticationService.getToken();
        this.authenticationService.login(this.username, this.password, token)
            .subscribe({
                next: (response: LoginResponse) => {
                    this.authenticationService.saveToken(response.token);
                    setTimeout(() => this.router.navigate(['/'], {replaceUrl: true}), 100);
                },
                error: (error: HttpErrorResponse) => {
                    this.errorMessage = this.getErrorMessage(error);
                }
            });
    }
}
