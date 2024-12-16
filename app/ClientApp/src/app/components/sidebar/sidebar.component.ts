import {Component} from '@angular/core';

interface MenuItem {
  path: string;
  label: string;
  icon: string;
}

@Component({
  selector: 'app-sidebar',
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
  `,
  styles: [`
    .sidebar {
      padding: 20px;
      height: 100%;
      border-right: 1px solid #dee2e6;
    }
    .nav-link {
      padding: 10px 15px;
      color: #333;
      display: flex;
      align-items: center;
      gap: 10px;
    }
    .nav-link.active {
      background-color: #e9ecef;
      border-radius: 4px;
    }
    .bi {
      font-size: 1.2rem;
    }
  `]
})
export class SidebarComponent {
  public readonly menuItems: MenuItem[] = [
    {path: './users', label: 'Users', icon: 'bi-people'},
    {path: './groups', label: 'Groups', icon: 'bi-collection'},
    {path: './roles', label: 'Roles', icon: 'bi-person-badge'},
    {path: './permissions', label: 'Permissions', icon: 'bi-shield-lock'}
  ];
}
