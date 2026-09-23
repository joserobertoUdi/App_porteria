import { InjectionToken } from '@angular/core';
import { Observable } from 'rxjs';
import { AprobacionesOperador } from '../models/aprobaciones';
import { HistorialParqueo, HistorialPorteria, HistorialUsuario } from '../models/historial';
import {
  ActualizarUsuarioParams,
  CrearUsuarioParams,
  UsuarioLista,
} from '../models/usuario';

export interface AdminRepository {
  listarUsuarios(): Observable<UsuarioLista[]>;
  crearUsuario(params: CrearUsuarioParams): Observable<number>;
  actualizarUsuario(id: number, params: ActualizarUsuarioParams): Observable<void>;
  restablecerPassword(id: number, nuevaPassword: string): Observable<void>;
  obtenerHistorialPorteria(): Observable<HistorialPorteria[]>;
  obtenerHistorialParqueo(): Observable<HistorialParqueo[]>;
  obtenerHistorialUsuario(id: number): Observable<HistorialUsuario>;
  obtenerAprobacionesOperador(id: number, fecha?: string): Observable<AprobacionesOperador>;
}

export const ADMIN_REPOSITORY = new InjectionToken<AdminRepository>('ADMIN_REPOSITORY');