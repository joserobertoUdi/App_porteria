import { Injectable } from '@angular/core';
import { UsuarioSesion } from '../../domain/models/usuario-sesion';

const SESION_KEY = 'udi_admin.sesion';

/**
 * Persistencia de la sesión en localStorage. La sesión incluye tokens y datos
 * del usuario para poder restaurarla al recargar el navegador.
 */
@Injectable({ providedIn: 'root' })
export class TokenStorage {
  leer(): UsuarioSesion | null {
    try {
      const raw = localStorage.getItem(SESION_KEY);
      return raw ? (JSON.parse(raw) as UsuarioSesion) : null;
    } catch {
      return null;
    }
  }

  guardar(sesion: UsuarioSesion): void {
    localStorage.setItem(SESION_KEY, JSON.stringify(sesion));
  }

  limpiar(): void {
    localStorage.removeItem(SESION_KEY);
  }
}