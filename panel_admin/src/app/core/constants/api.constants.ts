export const API = {
  /** Editar aquí para cambiar el servidor (patrón ApiConstants del proyecto Flutter). */
  baseUrl: 'http://localhost:5169',
  endpoints: {
    login: '/api/Auth/login',
    refresh: '/api/Auth/refresh',
    admin: {
      usuarios: '/api/Admin/usuarios',
      usuario: (id: number) => `/api/Admin/usuarios/${id}`,
      password: (id: number) => `/api/Admin/usuarios/${id}/password`,
      historialUsuario: (id: number) => `/api/Admin/usuarios/${id}/historial`,
      aprobaciones: (id: number) => `/api/Admin/usuarios/${id}/aprobaciones`,
      porteriaHistorial: '/api/Admin/porteria/historial',
      parqueoHistorial: '/api/Admin/parqueo/historial',
    },
  },
} as const;

/** Compone la URL absoluta a partir de la base y una ruta relativa. */
export function apiUrl(path: string): string {
  return `${API.baseUrl}${path}`;
}