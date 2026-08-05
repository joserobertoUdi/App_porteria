import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { AdminRemoteDataSource } from '../datasources/admin.remote.data-source';
import { HistorialParqueo, HistorialPorteria, HistorialUsuario } from '../../domain/models/historial';
import {
  ActualizarUsuarioParams,
  CrearUsuarioParams,
  UsuarioLista,
} from '../../domain/models/usuario';
import { AdminRepository } from '../../domain/repositories/admin.repository';

@Injectable({ providedIn: 'root' })
export class AdminRepositoryImpl implements AdminRepository {
  private readonly dataSource = inject(AdminRemoteDataSource);

  listarUsuarios(): Observable<UsuarioLista[]> {
    return this.dataSource.listarUsuarios();
  }

  crearUsuario(params: CrearUsuarioParams): Observable<number> {
    return this.dataSource.crearUsuario(params);
  }

  actualizarUsuario(id: number, params: ActualizarUsuarioParams): Observable<void> {
    return this.dataSource.actualizarUsuario(id, params);
  }

  restablecerPassword(id: number, nuevaPassword: string): Observable<void> {
    return this.dataSource.restablecerPassword(id, nuevaPassword);
  }

  obtenerHistorialPorteria(): Observable<HistorialPorteria[]> {
    return this.dataSource.obtenerHistorialPorteria();
  }

  obtenerHistorialParqueo(): Observable<HistorialParqueo[]> {
    return this.dataSource.obtenerHistorialParqueo();
  }

  obtenerHistorialUsuario(id: number): Observable<HistorialUsuario> {
    return this.dataSource.obtenerHistorialUsuario(id);
  }
}