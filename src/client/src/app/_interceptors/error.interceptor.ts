import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      catchError((err) => {
        if (
          [401, 403].includes(JSON.parse(err.status)) &&
          this.authService.accountValue
        ) {
          this.authService.logout();
        }
        const error = (err && err.error && err.error.message) || err.statusText;
        console.error(err);
        console.error(err.error);
        return throwError(() => error);
      })
    );
  }
}
