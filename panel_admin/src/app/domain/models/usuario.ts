export interface UsuarioLista {
  id: number;
  nombreCompleto: string;
  documentoIdentidad: string;
  tipoUsuario: string;
  rol: string;
  estado: boolean;
  fechaRegistro: string;
}

export interface CrearUsuarioParams {
  nombreCompleto: string;
  documentoIdentidad: string;
  tipoUsuarioId: number;
  password: string;
  rolId: number;
}

export interface ActualizarUsuarioParams {
  nombreCompleto: string;
  tipoUsuarioId: number;
  rolId: number;
  estado: boolean;
}