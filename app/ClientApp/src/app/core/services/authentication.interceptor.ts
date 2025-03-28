import {Injectable} from "@angular/core";
import {HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpErrorResponse} from "@angular/common/http";
import {AuthenticationService} from "./authentication.service";
import {Observable, throwError} from "rxjs";
import {catchError} from "rxjs/operators";
import {Constants} from "../constants";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
    constructor(private readonly authService: AuthenticationService) {
    }

    intercept(httpRequest: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        const token: string = this.authService.getToken();
        const isAuthEndpoint: boolean = httpRequest.url.includes(Constants.AUTHENTICATION);

        if (token && token.trim() !== '') {
            const modifiedRequest: HttpRequest<any> = httpRequest.clone({
                headers: httpRequest.headers.set('Authorization', `Bearer ${token}`)
            });
            
            return next.handle(modifiedRequest).pipe(
                catchError((error: HttpErrorResponse) => {
                    if (error.status === 401 && !isAuthEndpoint) {
                        this.authService.logout();
                    }
                    return throwError(() => error);
                })
            );
        }

        // Don't add token for authentication/login requests
        return next.handle(httpRequest);
    }
}
