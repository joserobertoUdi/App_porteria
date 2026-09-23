import {
  HttpErrorResponse,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, take, throwError } from 'rxjs';
import { API, apiUrl } from '../constants/api.constants';
import { AuthSessionService } from '../../presentation/services/auth-session.service';

function esEndpointAuth(url: string): boolean {
  const ruta = url.split('?')[0];
  return ruta === apiUrl(API.endpoints.login) || ruta === apiUrl(API.endpoints.refresh);
}

/**
 * Adjunta el Bearer token y, ante un 401, renueva la sesión una sola vez
 * (single-flight) y reintenta la petición original con el token nuevo.
 * Espejo del comportamiento de ApiClient en la app Flutter.
 */
export const authInterceptor: HttpInterceptorFn = (req: HttpRequest<unknown>, next: HttpHandlerFn) => {
  const sessionService = inject(AuthSessionService);
  const sesion = sessionService.sesion();

  let request = req;
  if (sesion?.token && !esEndpointAuth(req.url)) {
    request = req.clone({ setHeaders: { Authorization: `Bearer ${sesion.token}` } });
  }

  return next(request).pipe(
    catchError((err) => {
      if (err instanceof HttpErrorResponse && err.status === 401 && sesion && !esEndpointAuth(req.url)) {
        return sessionService.refresh().pipe(
          take(1),
          switchMap((renovada) => {
            if (!renovada) {
              sessionService.logout();
              return throwError(() => err);
            }
            const tokenNuevo = sessionService.sesion()?.token;
            const reintento = req.clone({
              setHeaders: { Authorization: `Bearer ${tokenNuevo}` },
            });
            return next(reintento);
          })
        );
      }
      return throwError(() => err);
    })
  );
};