const ROLES_VISIBLES: Record<string, string> = {
  Administrador: 'Administrador',
  PorteroPorteria: 'Portero Portería',
  PorteroParqueo: 'Portero Parqueo',
};

/** true si el rol corresponde a un guardia (portero de portería o de parqueo). */
export function esRolGuardia(rol: string): boolean {
  return rol.toLocaleLowerCase().includes('portero');
}

/** Nombre legible de un rol (el backend devuelve el nombre del enum). */
export function rolVisible(rol: string): string {
  return ROLES_VISIBLES[rol] ?? rol;
}