import { Injectable, signal } from '@angular/core';

export type TipoNotificacion = 'exito' | 'error' | 'info';

export interface Notificacion {
  mensaje: string;
  tipo: TipoNotificacion;
}

/** Aviso global tipo snackbar (patrón SnackBar de Flutter), no bloqueante. */
@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly actual = signal<Notificacion | null>(null);
  private timeoutId: ReturnType<typeof setTimeout> | null = null;

  mostrar(mensaje: string, tipo: TipoNotificacion = 'info'): void {
    this.actual.set({ mensaje, tipo });
    if (this.timeoutId) {
      clearTimeout(this.timeoutId);
    }
    this.timeoutId = setTimeout(() => this.actual.set(null), 3500);
  }

  exito(mensaje: string): void {
    this.mostrar(mensaje, 'exito');
  }

  error(mensaje: string): void {
    this.mostrar(mensaje, 'error');
  }

  ocultar(): void {
    this.actual.set(null);
  }
}