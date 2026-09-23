import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { API, apiUrl } from '../../core/constants/api.constants';
import { AprobacionesOperador } from '../../domain/models/aprobaciones';
import { HistorialParqueo, HistorialPorteria, HistorialUsuario } from '../../domain/models/historial';
import {
  ActualizarUsuarioParams,
  CrearUsuarioParams,
  UsuarioLista,
} from '../../domain/models/usuario';
import {
  AprobacionesOperadorDto,
  aprobacionesOperadorDto,
  HistorialParqueoDto,
  HistorialPorteriaDto,
  historialParqueoDto,
  historialPorteriaDto,
  HistorialUsuarioDto,
  historialUsuarioDto,
  UsuarioListaDto,
  usuarioListaDto,
} from '../models/admin.mappers';

export interface AdminDataSource {
  listarUsuarios(): Observable<UsuarioLista[]>;
  crearUsuario(params: CrearUsuarioParams): Observable<number>;
  actualizarUsuario(id: number, params: ActualizarUsuarioParams): Observable<void>;
  restablecerPassword(id: number, nuevaPassword: string): Observable<void>;
  obtenerHistorialPorteria(): Observable<HistorialPorteria[]>;
  obtenerHistorialParqueo(): Observable<HistorialParqueo[]>;
  obtenerHistorialUsuario(id: number): Observable<HistorialUsuario>;
  obtenerAprobacionesOperador(id: number, fecha?: string): Observable<AprobacionesOperador>;
}

@Injectable({ providedIn: 'root' })
export class AdminRemoteDataSource implements AdminDataSource {
  private readonly http = inject(HttpClient);

  listarUsuarios(): Observable<UsuarioLista[]> {
    return this.http
      .get<UsuarioListaDto[]>(apiUrl(API.endpoints.admin.usuarios))
      .pipe(map((lista) => lista.map(usuarioListaDto)));
  }

  crearUsuario(params: CrearUsuarioParams): Observable<number> {
    return this.http
      .post<{ id: number }>(apiUrl(API.endpoints.admin.usuarios), params)
      .pipe(map((r) => r.id));
  }

  actualizarUsuario(id: number, params: ActualizarUsuarioParams): Observable<void> {
    return this.http.put<void>(apiUrl(API.endpoints.admin.usuario(id)), params);
  }

  restablecerPassword(id: number, nuevaPassword: string): Observable<void> {
    return this.http.post<void>(apiUrl(API.endpoints.admin.password(id)), { nuevaPassword });
  }

  obtenerHistorialPorteria(): Observable<HistorialPorteria[]> {
    return this.http
      .get<HistorialPorteriaDto[]>(apiUrl(API.endpoints.admin.porteriaHistorial))
      .pipe(map((lista) => lista.map(historialPorteriaDto)));
  }

  obtenerHistorialParqueo(): Observable<HistorialParqueo[]> {
    return this.http
      .get<HistorialParqueoDto[]>(apiUrl(API.endpoints.admin.parqueoHistorial))
      .pipe(map((lista) => lista.map(historialParqueoDto)));
  }

  obtenerHistorialUsuario(id: number): Observable<HistorialUsuario> {
    return this.http
      .get<HistorialUsuarioDto>(apiUrl(API.endpoints.admin.historialUsuario(id)))
      .pipe(map(historialUsuarioDto));
  }

  obtenerAprobacionesOperador(id: number, fecha?: string): Observable<AprobacionesOperador> {
    const url =
      fecha !== undefined
        ? `${apiUrl(API.endpoints.admin.aprobaciones(id))}?fecha=${encodeURIComponent(fecha)}`
        : apiUrl(API.endpoints.admin.aprobaciones(id));
    return this.http
      .get<AprobacionesOperadorDto>(url)
      .pipe(map(aprobacionesOperadorDto));
  }
}