import { Routes } from '@angular/router';
import { AuthenticatedGuard } from './core/guards/authenticated.guard';
import { LoginComponent } from './login/login.component';

const routes: Routes = [
    {
        path: 'login',
        component: LoginComponent,
        canActivate: [AuthenticatedGuard]
    },
]; 