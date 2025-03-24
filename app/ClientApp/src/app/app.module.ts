import {BrowserModule} from '@angular/platform-browser';
import {NgModule} from '@angular/core';
import {FormsModule} from '@angular/forms';
import {HTTP_INTERCEPTORS, HttpClientModule} from '@angular/common/http';
import {AppRoutingModule} from './app-routing.module';

import {AppComponent} from './app.component';
import {NavMenuComponent} from './nav-menu/nav-menu.component';
import {HomeComponent} from './home/home.component';
import {LoginModule} from './login/login.module';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {ControlPanelModule} from './control-panel/control-panel.module';
import {AuthInterceptor} from './core/services/authentication.interceptor';
import {AuthAndAdminGuard} from './core/guards/auth-and-admin.guard';
import {AdminGuard} from './core/guards/admin.guard';

@NgModule({
    declarations: [
        AppComponent,
        NavMenuComponent,
        HomeComponent,
    ],
    imports: [
        BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
        HttpClientModule,
        FormsModule,
        LoginModule,
        AppRoutingModule,
        BrowserAnimationsModule,
        ControlPanelModule,
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
export class AppModule {
}
