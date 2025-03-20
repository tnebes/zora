import {NgModule} from '@angular/core';
import {RouterModule, Routes} from '@angular/router';
import {HomeComponent} from './home/home.component';
import {LoginComponent} from './login/login.component';
import {AuthGuard} from './core/guards/auth.guard';

const routes: Routes = [
    {path: '', component: HomeComponent, pathMatch: 'full'},
    {path: 'login', component: LoginComponent},
    {
        path: 'control-panel',
        loadChildren: () => import('./control-panel/control-panel.module').then(m => m.ControlPanelModule)
    },
    {
        path: 'tasks',
        loadChildren: () => import('./tasks/tasks.module').then(m => m.TasksModule),
        canActivate: [AuthGuard]
    }
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
    providers: [AuthGuard]
})
export class AppRoutingModule {
}
