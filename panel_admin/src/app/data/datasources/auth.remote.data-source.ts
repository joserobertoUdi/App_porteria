import { HttpBackend, HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API, apiUrl } from '../../core/constants/api.constants';
import { LoginParams, UsuarioSesion } from '../../domain/models/usuario-sesion';
import { LoginResponseDto, loginResponseToSesion } from '../models/admin.mappers';

export interface AuthDataSource {
  login(params: LoginParams): Observable<UsuarioSesion>;
  refresh(refreshToken: string): Observable<UsuarioSesion>;
}

/**
 * Datasource remoto de autenticación.
 * Usa un HttpClient "desnudo" (HttpBackend) que omite interceptores: login y refresh
 * son públicos, no llevan Authorization y así se evita recursión ante 401.
 */
@Injectable({ providedIn: 'root' })
export class AuthRemoteDataSource implements AuthDataSource {
  private readonly raw = new HttpClient(inject(HttpBackend));

  login(params: LoginParams): Observable<UsuarioSesion> {
    return this.raw
      .post<LoginResponseDto>(apiUrl(API.endpoints.login), params)
      .pipe(map(loginResponseToSesion));
  }

  refresh(refreshToken: string): Observable<UsuarioSesion> {
    return this.raw
      .post<LoginResponseDto>(apiUrl(API.endpoints.refresh), { refreshToken })
      .pipe(map(loginResponseToSesion));
  }
}