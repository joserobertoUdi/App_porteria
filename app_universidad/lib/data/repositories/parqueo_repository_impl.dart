import '../../domain/repositories/parqueo_repository.dart';
import '../datasources/parqueo_remote_data_source.dart';

class ParqueoRepositoryImpl implements ParqueoRepository {
  final ParqueoRemoteDataSource _dataSource;

  ParqueoRepositoryImpl(this._dataSource);

  @override
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
  }) async {
    final result = await _dataSource.registrarEntradaCompleta(
      documentoIdentidad: documentoIdentidad,
      nombreCompleto: nombreCompleto,
      tipoUsuarioId: tipoUsuarioId,
      matricula: matricula,
      marca: marca,
      modelo: modelo,
      color: color,
      puertaAcceso: puertaAcceso,
      observaciones: observaciones,
    );
    return {
      'id': result.id,
      'usuarioId': result.usuarioId,
      'vehiculoId': result.vehiculoId,
      'usuarioCreado': result.usuarioCreado,
      'vehiculoCreado': result.vehiculoCreado,
      'mensaje': result.mensaje,
    };
  }

  @override
  Future<void> registrarSalida(int id) async {
    await _dataSource.registrarSalida(id);
  }

  @override
  Future<List<Map<String, dynamic>>> obtenerActivos() async {
    final list = await _dataSource.obtenerActivos();
    return list.map((e) => {
      'idRegistro': e.idRegistro,
      'matricula': e.matricula,
      'marca': e.marca,
      'modelo': e.modelo,
      'color': e.color,
      'nombreCompleto': e.nombreCompleto,
      'documentoIdentidad': e.documentoIdentidad,
      'fechaIngreso': e.fechaIngreso.toIso8601String(),
      'puertaAcceso': e.puertaAcceso,
      'observaciones': e.observaciones,
    }).toList();
  }

  @override
  Future<Map<String, dynamic>?> buscarVehiculo(String placa) async {
    return await _dataSource.buscarVehiculo(placa);
  }
}
