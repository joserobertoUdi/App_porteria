import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthSessionService } from '../../presentation/services/auth-session.service';

/**
 * Protege las rutas del panel: exige sesión activa y rol Administrador.
 * El backend re-valida el rol en cada petición (defensa en profundidad).
 */
export const adminGuard: CanActivateFn = () => {
  const sessionService = inject(AuthSessionService);
  const router = inject(Router);

  if (sessionService.isAuthenticated() && sessionService.esAdmin()) {
    return true;
  }

  sessionService.logout();
  return router.parseUrl('/login');
};