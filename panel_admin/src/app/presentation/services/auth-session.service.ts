import { computed, inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, map, Observable, of, shareReplay, tap } from 'rxjs';
import { ROL_NOMBRE_ADMIN } from '../../core/constants/app.constants';
import { extraerMensajeError } from '../../core/services/api-error';
import { TokenStorage } from '../../core/services/token-storage';
import { LoginParams, UsuarioSesion } from '../../domain/models/usuario-sesion';
import { AUTH_REPOSITORY, AuthRepository } from '../../domain/repositories/auth.repository';

/**
 * Estado de sesión (equivalente a AuthProvider en Flutter). Fuente única de verdad
 * para autenticación; los componentes se suscriben reactivamente vía señales.
 */
@Injectable({ providedIn: 'root' })
export class AuthSessionService {
  private readonly repo = inject(AUTH_REPOSITORY) as AuthRepository;
  private readonly storage = inject(TokenStorage);
  private readonly router = inject(Router);

  private readonly _sesion = signal<UsuarioSesion | null>(this.storage.leer());
  readonly sesion = this._sesion.asReadonly();
  readonly isAuthenticated = computed(() => this._sesion() !== null);
  readonly esAdmin = computed(() => this._sesion()?.rolNombre === ROL_NOMBRE_ADMIN);

  readonly cargando = signal(false);
  readonly errorLogin = signal<string | null>(null);
  readonly bloqueadoHasta = signal<string | null>(null);

  private refrescoEnCurso: Observable<boolean> | null = null;

  /** Inicia sesión. Devuelve true si fue exitoso; en otro caso expone errorLogin. */
  login(params: LoginParams): Observable<boolean> {
    this.cargando.set(true);
    this.errorLogin.set(null);
    this.bloqueadoHasta.set(null);

    return this.repo.login(params).pipe(
      tap((sesion) => this.aplicarSesion(sesion)),
      map(() => true),
      finalize(() => this.cargando.set(false)),
      catchError((err) => {
        this.registrarErrorLogin(err);
        return of(false);
      })
    );
  }

  /**
   * Renueva la sesión ante 401. Es single-flight: peticiones concurrentes comparten
   * la misma renovación (shareReplay) y reintentan después. Al terminar se limpia
   * la referencia para permitir renovaciones futuras.
   */
  refresh(): Observable<boolean> {
    if (this.refrescoEnCurso) {
      return this.refrescoEnCurso;
    }

    const refreshToken = this._sesion()?.refreshToken;
    const flujo: Observable<boolean> = refreshToken
      ? this.repo.refresh(refreshToken).pipe(
          tap({
            next: (sesion) => this.aplicarSesion(sesion),
            error: () => this.cerrarSesionLocal(),
          }),
          map(() => true),
          catchError(() => of(false)),
          shareReplay({ bufferSize: 1, refCount: true }),
          finalize(() => (this.refrescoEnCurso = null))
        )
      : of(false);

    this.refrescoEnCurso = flujo;
    return flujo;
  }

  /** Registra el error del login sin exponer internos al componente. */
  registrarErrorLogin(err: unknown): void {
    const mensaje = extraerMensajeError(err);
    this.errorLogin.set(mensaje);
    this.bloqueadoHasta.set(null);
  }

  logout(): void {
    this.cerrarSesionLocal();
    void this.router.navigate(['/login']);
  }

  private aplicarSesion(sesion: UsuarioSesion): void {
    this._sesion.set(sesion);
    this.storage.guardar(sesion);
  }

  private cerrarSesionLocal(): void {
    this._sesion.set(null);
    this.storage.limpiar();
  }
}