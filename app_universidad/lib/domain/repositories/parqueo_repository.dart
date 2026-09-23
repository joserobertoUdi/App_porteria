abstract class ParqueoRepository {
  Future<Map<String, dynamic>> registrarEntradaCompleta({
    required String documentoIdentidad,
    String? nombreCompleto,
    String? tipoUsuarioId,
    required String matricula,
    String? marca,
    String? modelo,
    String? color,
    required String puertaAcceso,
    String? observaciones,
  });
  Future<void> registrarSalida(int id);
  Future<List<Map<String, dynamic>>> obtenerActivos();
  Future<Map<String, dynamic>?> buscarVehiculo(String placa);
}
