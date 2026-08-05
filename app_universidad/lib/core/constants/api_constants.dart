class ApiConstants {
  static const String baseUrl = 'http://100.106.35.85:5169';

  // Auth
  static const String login = '/api/Auth/login';

  // Usuarios
  static const String usuarioBase = '/api/Usuario';
  static String buscarUsuario(String dni) => '$usuarioBase/buscar/$dni';
  static String sugerirUsuarios(String termino) => '$usuarioBase/sugerir/$termino';

  // Porteria
  static const String porteriaBase = '/api/Porteria';
  static const String porteriaEntrada = '$porteriaBase/entrada';
  static const String porteriaEntradaCompleta = '$porteriaBase/entrada-completa';
  static const String porteriaSalida = '$porteriaBase/salida';
  static const String porteriaActivos = '$porteriaBase/activos';

  // Admin
  static const String adminBase = '/api/Admin';
  static const String adminUsuarios = '$adminBase/usuarios';
  static const String adminHistorialPorteria = '$adminBase/porteria/historial';
  static const String adminHistorialParqueo = '$adminBase/parqueo/historial';

  // Parqueo
  static const String parqueoBase = '/api/Parqueo';
  static const String parqueoEntradaCompleta = '$parqueoBase/entrada-completa';
  static const String parqueoEntrada = '$parqueoBase/entrada';
  static const String parqueoSalida = '$parqueoBase/salida';
  static const String parqueoActivos = '$parqueoBase/activos';
  static String parqueoHistorial(int usuarioId) => '$parqueoBase/historial/$usuarioId';
  static String buscarVehiculo(String placa) => '$parqueoBase/vehiculos/buscar-placa/$placa';
  static String vehiculosPorUsuario(int usuarioId) => '$parqueoBase/vehiculos/$usuarioId';
}
