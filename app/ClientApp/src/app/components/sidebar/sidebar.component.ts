import {Component} from '@angular/core';

interface MenuItem {
    path: string;
    label: string;
    icon: string;
}

@Component({
    selector: 'app-sidebar',
    styleUrls: ['./sidebar.component.scss'],
    template: `
    <nav class="sidebar">
      <ul class="nav flex-column">
        <li *ngFor="let item of menuItems" class="nav-item">
          <a class="nav-link"
             [routerLink]="[item.path]"
             routerLinkActive="active">
            <i class="bi" [class]="item.icon"></i>
            {{ item.label }}
          </a>
        </li>
      </ul>
    </nav>
  `
})
export class SidebarComponent {
    public readonly menuItems: MenuItem[] = [
        {path: './users', label: 'Users', icon: 'bi-people'},
        {path: './roles', label: 'Roles', icon: 'bi-person-badge'},
        {path: './permissions', label: 'Permissions', icon: 'bi-shield-lock'},
        {path: './assets', label: 'Assets', icon: 'bi-box'}
    ];
}
