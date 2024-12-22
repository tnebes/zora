import {Injectable} from "@angular/core";
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest} from "@angular/common/http";
import {AuthenticationService} from "./authentication.service";
import {Observable} from "rxjs";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private readonly authService: AuthenticationService) {
    }

    intercept(httpRequest: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token: string = this.authService.getToken();

        if (token) {
            const modifiedRequest: HttpRequest<any> = httpRequest.clone({
                headers: httpRequest.headers.set('Authorization', `Bearer ${token}`)
            });
            return next.handle(modifiedRequest);
        }

        return next.handle(httpRequest);
    }
}
