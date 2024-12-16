import {Component, OnInit} from '@angular/core';
import {Router} from '@angular/router';
import {AuthenticationService} from '../core/services/authentication.service';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.css']
})
export class NavMenuComponent implements OnInit {
  public isExpanded: boolean = false;
  public isAuthenticated: boolean = false;
  public isAdmin: boolean = false;

  constructor(private readonly authenticationService: AuthenticationService, private readonly router: Router) {
  }

  public ngOnInit(): void {
    this.authenticationService.authState$.subscribe((isAuthenticated: boolean) => {
      this.isAuthenticated = isAuthenticated;
    });
    this.authenticationService.isAdmin$.subscribe((isAdmin: boolean) => {
      this.isAdmin = isAdmin;
    });

    this.authenticationService.checkAuthStatus().subscribe({
      error: () => {
      }
    });
  }

  public collapse() {
    this.isExpanded = false;
  }

  public toggle() {
    this.isExpanded = !this.isExpanded;
  }

  public logout(): void {
    this.authenticationService.logout();
    this.router.navigate(['/']);
  }
}
