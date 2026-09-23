export interface HistorialPorteria {
  idRegistro: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  fechaEntrada: string;
  fechaSalida: string | null;
  puertaEntrada: string | null;
  puertaSalida: string | null;
  motivoVisita: string | null;
  areaDestino: string | null;
  registradoPorNombre: string | null;
  registradoPorDocumento: string | null;
}

export interface HistorialParqueo {
  idRegistro: number;
  matricula: string;
  marca: string | null;
  modelo: string | null;
  color: string | null;
  nombreCompleto: string;
  documentoIdentidad: string;
  fechaIngreso: string;
  fechaSalida: string | null;
  puertaAcceso: string;
  observaciones: string | null;
  registradoPorNombre: string | null;
  registradoPorDocumento: string | null;
}

export interface HistorialUsuario {
  nombreCompleto: string;
  documentoIdentidad: string;
  porteria: HistorialPorteria[];
  parqueo: HistorialParqueo[];
}