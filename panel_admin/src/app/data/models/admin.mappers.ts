import { HistorialParqueo, HistorialPorteria, HistorialUsuario } from '../../domain/models/historial';
import { UsuarioLista } from '../../domain/models/usuario';
import { UsuarioSesion } from '../../domain/models/usuario-sesion';

/** Shapes del wire (camelCase, convención JSON de ASP.NET Core). */
export interface LoginResponseDto {
  token: string;
  refreshToken: string | null;
  expiraEnMinutos: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  tipoUsuarioId: number;
  rolId: number | null;
  rolNombre: string;
  fotoUrl: string | null;
}

export interface UsuarioListaDto {
  id: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  tipoUsuario: string;
  rol: string;
  estado: boolean;
  fechaRegistro: string;
}

export interface HistorialPorteriaDto {
  idRegistro: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  fechaEntrada: string;
  fechaSalida: string | null;
  puertaEntrada: string | null;
  puertaSalida: string | null;
  motivoVisita: string | null;
  areaDestino: string | null;
}

export interface HistorialParqueoDto {
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
}

export interface HistorialUsuarioDto {
  nombreCompleto: string;
  documentoIdentidad: string;
  porteria: HistorialPorteriaDto[];
  parqueo: HistorialParqueoDto[];
}

export function loginResponseToSesion(dto: LoginResponseDto): UsuarioSesion {
  return {
    token: dto.token,
    refreshToken: dto.refreshToken,
    expiraEnMinutos: dto.expiraEnMinutos,
    nombreCompleto: dto.nombreCompleto,
    documentoIdentidad: dto.documentoIdentidad,
    tipoUsuarioId: dto.tipoUsuarioId,
    rolId: dto.rolId,
    rolNombre: dto.rolNombre,
  };
}

export function usuarioListaDto(dto: UsuarioListaDto): UsuarioLista {
  return {
    id: dto.id,
    nombreCompleto: dto.nombreCompleto,
    documentoIdentidad: dto.documentoIdentidad,
    tipoUsuario: dto.tipoUsuario,
    rol: dto.rol,
    estado: dto.estado,
    fechaRegistro: dto.fechaRegistro,
  };
}

export function historialPorteriaDto(dto: HistorialPorteriaDto): HistorialPorteria {
  return {
    idRegistro: dto.idRegistro,
    nombreCompleto: dto.nombreCompleto,
    documentoIdentidad: dto.documentoIdentidad,
    fechaEntrada: dto.fechaEntrada,
    fechaSalida: dto.fechaSalida,
    puertaEntrada: dto.puertaEntrada,
    puertaSalida: dto.puertaSalida,
    motivoVisita: dto.motivoVisita,
    areaDestino: dto.areaDestino,
  };
}

export function historialParqueoDto(dto: HistorialParqueoDto): HistorialParqueo {
  return {
    idRegistro: dto.idRegistro,
    matricula: dto.matricula,
    marca: dto.marca,
    modelo: dto.modelo,
    color: dto.color,
    nombreCompleto: dto.nombreCompleto,
    documentoIdentidad: dto.documentoIdentidad,
    fechaIngreso: dto.fechaIngreso,
    fechaSalida: dto.fechaSalida,
    puertaAcceso: dto.puertaAcceso,
    observaciones: dto.observaciones,
  };
}

export function historialUsuarioDto(dto: HistorialUsuarioDto): HistorialUsuario {
  return {
    nombreCompleto: dto.nombreCompleto,
    documentoIdentidad: dto.documentoIdentidad,
    porteria: (dto.porteria ?? []).map(historialPorteriaDto),
    parqueo: (dto.parqueo ?? []).map(historialParqueoDto),
  };
}