import '../../core/network/api_client.dart';
import '../models/hora_servidor_model.dart';

class SistemaRemoteDataSource {
  final ApiClient client;

  SistemaRemoteDataSource(this.client);

  /// Fuente de verdad: hora actual del servidor (UTC y local) y su zona horaria.
  Future<HoraServidorModel> obtenerHoraServidor() async {
    final response = await client.get('/api/Sistema/hora');
    return HoraServidorModel.fromJson(response);
  }
}
