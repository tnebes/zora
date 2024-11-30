import {Component} from '@angular/core';
import {AuthService, LoginResponse} from '../core/services/authentication.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  username: string = '';
  password: string = '';

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
  }

  public onLogin(): void {
    this.authService.login(this.username, this.password)
      .subscribe({
        next: (response: LoginResponse) => {
          this.authService.saveToken(response.token);
          this.router.navigate(['/'], {replaceUrl: true});
        },
        error: (error: Error) => {
          console.error('Login failed:', error);
        }
      });
  }
}
