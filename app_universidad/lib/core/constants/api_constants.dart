class ApiConstants {
  /// URL de la API de Servicios Generales.
  ///
  /// `localhost` solo sirve cuando la app corre en el mismo equipo que la API.
  /// En un teléfono físico, `localhost` es el propio teléfono, así que hay que
  /// usar la IP del PC en la red local. Se puede sobrescribir sin tocar el
  /// código:
  ///
  ///     flutter run --dart-define=API_BASE_URL=http://192.168.137.1:5169
  ///
  /// Para el emulador de Android, la IP del equipo anfitrión es 10.0.2.2.
  static const String baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    // Servidor de pruebas. Es el mismo host donde corre SharepointApi, así que
    // quien alcanza una API alcanza la otra. Va bajo IIS en el puerto 80 con
    // ruta virtual, de ahí que no lleve número de puerto.
    defaultValue: 'http://10.1.210.10/ServiciosGenerales',
  );

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
