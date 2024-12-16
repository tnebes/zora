import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ControlPanelComponent } from './control-panel.component';
import { SidebarComponent } from '../components/sidebar.component';
import { UsersComponent } from '../components/users.component';
import { GroupsComponent } from '../components/groups.component';
import { RolesComponent } from '../components/roles.component';
import { PermissionsComponent } from '../components/permissions.component';

@NgModule({
  declarations: [
    ControlPanelComponent,
    SidebarComponent,
    UsersComponent,
    GroupsComponent,
    RolesComponent,
    PermissionsComponent
  ],
  imports: [
    CommonModule,
    RouterModule.forChild([
      {
        path: '',
        component: ControlPanelComponent,
        children: [
          { path: '', redirectTo: 'users', pathMatch: 'full' },
          { path: 'users', component: UsersComponent },
          { path: 'groups', component: GroupsComponent },
          { path: 'roles', component: RolesComponent },
          { path: 'permissions', component: PermissionsComponent }
        ]
      }
    ])
  ]
})
export class ControlPanelModule { }