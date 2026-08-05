import { inject, Injectable, signal } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import {
  catchError,
  combineLatest,
  map,
  Observable,
  of,
  shareReplay,
  startWith,
  Subject,
  switchMap,
} from 'rxjs';
import { FUENTES, FuenteHistorial } from '../../core/constants/app.constants';
import { extraerMensajeError } from '../../core/services/api-error';
import { ToastService } from '../../core/services/toast.service';
import { HistorialParqueo, HistorialPorteria, HistorialUsuario } from '../../domain/models/historial';
import {
  ActualizarUsuarioParams,
  CrearUsuarioParams,
  UsuarioLista,
} from '../../domain/models/usuario';
import { AdminRepository, ADMIN_REPOSITORY } from '../../domain/repositories/admin.repository';

export type RegistroHistorial = HistorialPorteria | HistorialParqueo;

/**
 * Orquesta la gestión de administración (usuarios e historiales) de forma reactiva.
 * Expone observables fríos compartidos (shareReplay) para que la UI nunca bloquee.
 */
@Injectable({ providedIn: 'root' })
export class AdminService {
  private readonly repo = inject(ADMIN_REPOSITORY) as AdminRepository;
  private readonly toast = inject(ToastService);

  /** Fuente de historial activa (portería o parqueo). */
  readonly fuente = signal<FuenteHistorial>(FUENTES.porteria);

  private readonly recargaUsuarios = new Subject<void>();
  private readonly recargaHistorial = new Subject<void>();

  /** Lista de usuarios del sistema (se recarga con recargarUsuarios). */
  readonly usuarios$ = this.recargaUsuarios.pipe(
    startWith(void 0),
    switchMap(() =>
      this.repo.listarUsuarios().pipe(
        catchError((err) => {
          this.toast.error(extraerMensajeError(err));
          return of<UsuarioLista[]>([]);
        })
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  /** Historial según la fuente activa (reacciona a fuente y a recargarHistorial). */
  readonly historial$ = combineLatest([
    toObservable(this.fuente),
    this.recargaHistorial.pipe(startWith(undefined)),
  ]).pipe(
    switchMap(([fuente]) => this.obtenerHistorial(fuente)),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  recargarUsuarios(): void {
    this.recargaUsuarios.next();
  }

  recargarHistorial(): void {
    this.recargaHistorial.next();
  }

  cambiarFuente(fuente: FuenteHistorial): void {
    this.fuente.set(fuente);
  }

  crearUsuario(params: CrearUsuarioParams): Observable<boolean> {
    return this.repo.crearUsuario(params).pipe(
      map(() => {
        this.toast.exito('Usuario creado correctamente');
        this.recargarUsuarios();
        return true;
      }),
      catchError((err) => {
        this.toast.error(extraerMensajeError(err));
        return of(false);
      })
    );
  }

  actualizarUsuario(id: number, params: ActualizarUsuarioParams): Observable<boolean> {
    return this.repo.actualizarUsuario(id, params).pipe(
      map(() => {
        this.toast.exito('Usuario actualizado correctamente');
        this.recargarUsuarios();
        return true;
      }),
      catchError((err) => {
        this.toast.error(extraerMensajeError(err));
        return of(false);
      })
    );
  }

  restablecerPassword(id: number, nuevaPassword: string): Observable<boolean> {
    return this.repo.restablecerPassword(id, nuevaPassword).pipe(
      map(() => {
        this.toast.exito('Contraseña restablecida correctamente');
        return true;
      }),
      catchError((err) => {
        this.toast.error(extraerMensajeError(err));
        return of(false);
      })
    );
  }

  /** Historial de visitas (portería y parqueo) de un usuario. */
  obtenerHistorialUsuario(id: number): Observable<HistorialUsuario> {
    return this.repo.obtenerHistorialUsuario(id).pipe(
      catchError((err) => {
        this.toast.error(extraerMensajeError(err));
        throw err;
      })
    );
  }

  private obtenerHistorial(fuente: FuenteHistorial): Observable<RegistroHistorial[]> {
    const peticion: Observable<RegistroHistorial[]> =
      fuente === FUENTES.porteria
        ? this.repo.obtenerHistorialPorteria()
        : this.repo.obtenerHistorialParqueo();

    return peticion.pipe(
      catchError((err) => {
        this.toast.error(extraerMensajeError(err));
        return of([]);
      })
    );
  }
}