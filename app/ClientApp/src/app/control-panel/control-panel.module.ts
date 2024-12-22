import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';
import {RouterModule} from '@angular/router';
import {MatTableModule} from '@angular/material/table';
import {MatPaginatorModule} from '@angular/material/paginator';
import {MatSortModule} from '@angular/material/sort';
import {MatInputModule} from '@angular/material/input';
import {MatIconModule} from '@angular/material/icon';
import {MatButtonModule} from '@angular/material/button';
import {MatFormFieldModule} from '@angular/material/form-field';

import {ControlPanelComponent} from './control-panel.component';
import {SidebarComponent} from '../components/sidebar/sidebar.component';
import {UsersComponent} from '../components/users/users.component';
import {GroupsComponent} from '../components/groups/groups.component';
import {RolesComponent} from '../components/roles/roles.component';
import {PermissionsComponent} from '../components/permissions/permissions.component';
import {MatDialogModule} from '@angular/material/dialog';
import {SharedModule} from '../shared/shared.module';

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
        MatTableModule,
        MatPaginatorModule,
        MatSortModule,
        MatInputModule,
        MatIconModule,
        MatButtonModule,
        MatFormFieldModule,
        MatDialogModule,
        SharedModule,
        RouterModule.forChild([
            {
                path: '',
                component: ControlPanelComponent,
                children: [
                    {path: '', redirectTo: 'users', pathMatch: 'full'},
                    {path: 'users', component: UsersComponent},
                    {path: 'groups', component: GroupsComponent},
                    {path: 'roles', component: RolesComponent},
                    {path: 'permissions', component: PermissionsComponent}
                ]
            }
        ])
    ]
})
export class ControlPanelModule {
}
