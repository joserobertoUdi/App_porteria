class LoginResponseModel {
  final String token;
  final String refreshToken;
  final String nombreCompleto;
  final String documentoIdentidad;
  final int tipoUsuarioId;
  final int rolId;
  final String rolNombre;
  final String? fotoUrl;
  final int expiraEnMinutos;

  LoginResponseModel({
    required this.token,
    required this.refreshToken,
    required this.nombreCompleto,
    required this.documentoIdentidad,
    required this.tipoUsuarioId,
    required this.rolId,
    required this.rolNombre,
    this.fotoUrl,
    this.expiraEnMinutos = 720,
  });

  factory LoginResponseModel.fromJson(Map<String, dynamic> json) {
    return LoginResponseModel(
      token: json['token'] ?? '',
      refreshToken: json['refreshToken'] ?? '',
      nombreCompleto: json['nombreCompleto'] ?? '',
      documentoIdentidad: json['documentoIdentidad'] ?? '',
      tipoUsuarioId: json['tipoUsuarioId'] ?? 0,
      rolId: json['rolId'] ?? 0,
      rolNombre: json['rolNombre'] ?? '',
      fotoUrl: json['fotoUrl'],
      expiraEnMinutos: json['expiraEnMinutos'] ?? 720,
    );
  }
}
