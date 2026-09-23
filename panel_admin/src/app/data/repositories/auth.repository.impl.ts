import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthRemoteDataSource } from '../datasources/auth.remote.data-source';
import { LoginParams, UsuarioSesion } from '../../domain/models/usuario-sesion';
import { AuthRepository } from '../../domain/repositories/auth.repository';

@Injectable({ providedIn: 'root' })
export class AuthRepositoryImpl implements AuthRepository {
  private readonly dataSource = inject(AuthRemoteDataSource);

  login(params: LoginParams): Observable<UsuarioSesion> {
    return this.dataSource.login(params);
  }

  refresh(refreshToken: string): Observable<UsuarioSesion> {
    return this.dataSource.refresh(refreshToken);
  }
}