import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { LoginParams, UsuarioSesion } from '../models/usuario-sesion';

export interface AuthRepository {
  login(params: LoginParams): Observable<UsuarioSesion>;
  refresh(refreshToken: string): Observable<UsuarioSesion>;
}

export const AUTH_REPOSITORY = new InjectionToken<AuthRepository>('AUTH_REPOSITORY');