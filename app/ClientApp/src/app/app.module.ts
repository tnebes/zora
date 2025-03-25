import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {AppRoutingModule} from './app-routing.module';

import {AppComponent} from './app.component';
import {NavMenuComponent} from './nav-menu/nav-menu.component';
import {LoginModule} from './login/login.module';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {ControlPanelModule} from './control-panel/control-panel.module';
import {AuthInterceptor} from './core/services/authentication.interceptor';
import {AuthAndAdminGuard} from './core/guards/auth-and-admin.guard';
import {AdminGuard} from './core/guards/admin.guard';
import {HomeModule} from './home/home.module';
import { ProfileComponent } from './profile/profile.component';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        ProfileComponent
    ],
    imports: [
        BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
        HttpClientModule,
        FormsModule,
        LoginModule,
        AppRoutingModule,
        BrowserAnimationsModule,
        ControlPanelModule,
        HomeModule,
        MatCardModule,
        MatIconModule
    ],
    providers: [
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        },
        AuthAndAdminGuard,
        AdminGuard
    ],
    bootstrap: [AppComponent]
})
export class AppModule {}
