import '../../core/network/api_client.dart';
import '../../core/services/logger_service.dart';
import '../models/parqueo_models.dart';

class ParqueoRemoteDataSource {
  final ApiClient client;

  ParqueoRemoteDataSource(this.client);

  Future<EntradaCompletaResultadoModel> registrarEntradaCompleta({
    required String documentoIdentidad,
    String? nombreCompleto,
    String? tipoUsuarioId,
    required String matricula,
    String? marca,
    String? modelo,
    String? color,
    required String puertaAcceso,
    String? observaciones,
  }) async {
    final response = await client.post('/api/Parqueo/entrada-completa', {
      'documentoIdentidad': documentoIdentidad,
      'nombreCompleto': nombreCompleto,
      'tipoUsuarioId': tipoUsuarioId,
      'matricula': matricula,
      'marca': marca,
      'modelo': modelo,
      'color': color,
      'puertaAcceso': puertaAcceso,
      'observaciones': observaciones,
    });
    return EntradaCompletaResultadoModel.fromJson(response);
  }

  Future<void> registrarSalida(int id) async {
    await client.post('/api/Parqueo/salida', {'id': id});
  }

  Future<List<ParqueoActivoModel>> obtenerActivos() async {
    final list = await client.getList('/api/Parqueo/activos');
    return list.map((e) => ParqueoActivoModel.fromJson(e as Map<String, dynamic>)).toList();
  }

  Future<Map<String, dynamic>?> buscarVehiculo(String placa) async {
    try {
      final response = await client.get('/api/Parqueo/vehiculos/buscar-placa/$placa');
      return response;
    } catch (e) {
      // Se devuelve null para que el llamador maneje el caso "no encontrado"
      // sin propagar excepción. El error se registra para diagnóstico.
      LoggerService.warning('buscarVehiculo($placa): $e');
      return null;
    }
  }

  Future<List<Map<String, dynamic>>> obtenerVehiculosPorUsuario(int usuarioId) async {
    final list = await client.getList('/api/Parqueo/vehiculos/$usuarioId');
    return list.cast<Map<String, dynamic>>();
  }
}
