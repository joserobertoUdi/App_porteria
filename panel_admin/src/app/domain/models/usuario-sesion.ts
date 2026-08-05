export interface UsuarioSesion {
  token: string;
  refreshToken: string | null;
  expiraEnMinutos: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  tipoUsuarioId: number;
  rolId: number | null;
  rolNombre: string;
}

export interface LoginParams {
  documentoIdentidad: string;
  password: string;
}