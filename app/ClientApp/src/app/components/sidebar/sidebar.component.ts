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
            <mat-icon>{{ item.icon }}</mat-icon>
            {{ item.label }}
          </a>
        </li>
      </ul>
    </nav>
  `
})
export class SidebarComponent {
    public readonly menuItems: MenuItem[] = [
        {path: './users', label: 'Users', icon: 'people'},
        {path: './roles', label: 'Roles', icon: 'admin_panel_settings'},
        {path: './permissions', label: 'Permissions', icon: 'lock'},
        {path: './assets', label: 'Assets', icon: 'inventory'}
    ];
}
