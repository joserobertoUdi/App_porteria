import { HttpErrorResponse } from '@angular/common/http';

export class ApiError extends Error {
  readonly status: number;
  readonly bloqueadoHasta?: string;

  constructor(message: string, status: number, bloqueadoHasta?: string) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.bloqueadoHasta = bloqueadoHasta;
  }
}

interface CuerpoError {
  mensaje?: string;
  error?: string;
  title?: string;
  bloqueadoHasta?: string;
}

/** Extrae un mensaje legible de cualquier error HTTP del backend UDI. */
export function extraerMensajeError(err: unknown): string {
  if (err instanceof ApiError) {
    return err.message;
  }
  if (err instanceof HttpErrorResponse) {
    const cuerpo = err.error as CuerpoError | null;
    if (cuerpo) {
      const mensaje = cuerpo.mensaje ?? cuerpo.error ?? cuerpo.title;
      if (mensaje) {
        return mensaje;
      }
      if (err.status === 429) {
        return 'Demasiadas peticiones. Intente nuevamente en un momento.';
      }
    }
    switch (err.status) {
      case 0:
        return 'No se pudo conectar con el servidor.';
      case 401:
        return 'Sesión no autorizada.';
      case 403:
        return 'Acceso denegado.';
      case 404:
        return 'Recurso no encontrado.';
      default:
        return 'Ocurrió un error inesperado.';
    }
  }
  if (err instanceof Error && err.message) {
    return err.message;
  }
  return 'Ocurrió un error inesperado.';
}