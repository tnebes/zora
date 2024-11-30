import {Component} from '@angular/core';
import {AuthService} from '../core/services/authentication.service';
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

  onLogin() {
    console.log('Submitting:', {username: this.username, password: this.password});
    this.authService.login(this.username, this.password)
      .subscribe({
        next: (token) => {
          this.authService.saveToken(token);
          this.router.navigate(['/'], {replaceUrl: true});
        },
        error: (error) => {
          console.error('Login failed:', error);
        }
      });
  }
}
