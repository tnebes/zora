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
import {MatDialogModule} from '@angular/material/dialog';
import {SharedModule} from '../shared/shared.module';

import {ControlPanelComponent} from './control-panel.component';
import {SidebarComponent} from '../components/sidebar/sidebar.component';
import {UsersComponent} from '../components/users/users.component';
import {AssetsComponent} from '../components/assets/assets.component';
import {RolesComponent} from '../components/roles/roles.component';
import {PermissionsComponent} from '../components/permissions/permissions.component';

import {AuthAndAdminGuard} from '../core/guards/auth-and-admin.guard';
import {AdminGuard} from '../core/guards/admin.guard';
import { MatTooltipModule } from '@angular/material/tooltip';

@NgModule({
    declarations: [
        ControlPanelComponent,
        SidebarComponent,
        UsersComponent,
        AssetsComponent,
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
        MatTooltipModule,
        RouterModule.forChild([
            {
                path: '',
                component: ControlPanelComponent,
                canActivate: [AuthAndAdminGuard],
                children: [
                    {path: '', redirectTo: 'users', pathMatch: 'full'},
                    {path: 'users', component: UsersComponent, canActivate: [AdminGuard]},
                    {path: 'assets', component: AssetsComponent, canActivate: [AdminGuard]},
                    {path: 'roles', component: RolesComponent, canActivate: [AdminGuard]},
                    {path: 'permissions', component: PermissionsComponent, canActivate: [AdminGuard]}
                ]
            }
        ])
    ]
})
export class ControlPanelModule {
}
