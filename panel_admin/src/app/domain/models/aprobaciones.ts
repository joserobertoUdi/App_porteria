export interface AprobacionPorteria {
  idRegistro: number;
  fechaEntrada: string;
  fechaSalida: string | null;
  puertaEntrada: string | null;
  puertaSalida: string | null;
  motivoVisita: string | null;
  areaDestino: string | null;
  visitanteNombre: string;
  visitanteDocumento: string;
  estaCompletada: boolean;
}

export interface AprobacionParqueo {
  idRegistro: number;
  fechaIngreso: string;
  fechaSalida: string | null;
  puertaAcceso: string;
  matricula: string;
  marca: string | null;
  modelo: string | null;
  visitanteNombre: string;
  visitanteDocumento: string;
  estaCompletada: boolean;
}

export interface AprobacionesOperador {
  usuarioId: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  rol: string;
  desde: string | null;
  hasta: string | null;
  totalCompletadas: number;
  totalPendientes: number;
  porteria: AprobacionPorteria[];
  parqueo: AprobacionParqueo[];
}