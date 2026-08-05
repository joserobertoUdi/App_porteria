/** Formatea una fecha ISO (ISO 8601 del backend) a dd/MM/yyyy HH:mm local. */
export function formatearFecha(valor: string | null | undefined): string {
  if (!valor) {
    return '—';
  }
  const fecha = new Date(valor);
  if (Number.isNaN(fecha.getTime())) {
    return valor;
  }
  return fecha.toLocaleString('es-ES', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

/** Iniciales de un nombre para el avatar. */
export function iniciales(nombre: string): string {
  const partes = nombre.trim().split(/\s+/).filter(Boolean);
  if (partes.length === 0) {
    return '?';
  }
  const inicial = partes[0].charAt(0);
  const segunda = partes.length > 1 ? partes[1].charAt(0) : '';
  return (inicial + segunda).toUpperCase();
}