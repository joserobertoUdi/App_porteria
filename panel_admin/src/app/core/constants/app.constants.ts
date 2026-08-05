export const ROLES = {
  Administrador: 1,
  PorteroParqueo: 2,
  PorteroPorteria: 3,
} as const;

export const ROL_NOMBRE_ADMIN = 'Administrador' as const;

export const TIPOS_USUARIO = {
  Estudiante: 1,
  Trabajador: 2,
  Visitante: 3,
} as const;

export const CATALOGO_TIPOS_USUARIO = [
  { id: TIPOS_USUARIO.Estudiante, nombre: 'Estudiante' },
  { id: TIPOS_USUARIO.Trabajador, nombre: 'Trabajador' },
  { id: TIPOS_USUARIO.Visitante, nombre: 'Visitante' },
] as const;

export const CATALOGO_ROLES = [
  { id: ROLES.Administrador, nombre: 'Administrador' },
  { id: ROLES.PorteroParqueo, nombre: 'Portero Parqueo' },
  { id: ROLES.PorteroPorteria, nombre: 'Portero Portería' },
] as const;

/** Filtro de tipo de vista en el historial. */
export const FILTROS_ESTADO = {
  todos: 'todos',
  activos: 'activos',
  completados: 'completados',
} as const;

export type FiltroEstado = (typeof FILTROS_ESTADO)[keyof typeof FILTROS_ESTADO];

/** Segmentos del historial: portería y parqueo. */
export const FUENTES = {
  porteria: 'porteria',
  parqueo: 'parqueo',
} as const;

export type FuenteHistorial = (typeof FUENTES)[keyof typeof FUENTES];

export const ETIQUETAS_FUENTE: Record<FuenteHistorial, string> = {
  [FUENTES.porteria]: 'Portería',
  [FUENTES.parqueo]: 'Parqueo',
};