import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants/error_messages.dart';
import '../../core/network/api_client.dart';
import '../../core/services/logger_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/error_view.dart';
import '../widgets/udi_appbar_logo.dart';

class HistorialParqueoPage extends StatefulWidget {
  const HistorialParqueoPage({super.key});

  @override
  State<HistorialParqueoPage> createState() => _HistorialParqueoPageState();
}

class _HistorialParqueoPageState extends State<HistorialParqueoPage> {
  List<Map<String, dynamic>> _registros = [];
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargarHistorial();
  }

  Future<void> _cargarHistorial() async {
    setState(() {
      _cargando = true;
      _error = null;
    });
    final api = context.read<ApiClient>();
    try {
      final list = await api.getList('/api/Admin/parqueo/historial');
      if (!mounted) return;
      setState(() => _registros = list.cast<Map<String, dynamic>>());
    } catch (e, st) {
      LoggerService.error('Al cargar el historial de parqueo', e, st);
      if (!mounted) return;
      setState(() {
        _registros = [];
        _error = mensajeDeError(e);
      });
    } finally {
      if (mounted) setState(() => _cargando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const UdiAppBarLogo(width: 40),
            const SizedBox(width: 8),
            const Text('Historial Parqueo', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ],
        ),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _cargarHistorial,
          ),
        ],
      ),
      body: _cargando
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? ErrorView(message: _error!, onRetry: _cargarHistorial)
              : _registros.isEmpty
                  ? const Center(child: Text('Sin registros'))
                  : RefreshIndicator(
                  onRefresh: _cargarHistorial,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(10),
                    itemCount: _registros.length,
                    itemBuilder: (context, index) {
                      final r = _registros[index];
                      final fechaSalida = r['fechaSalida'];
                      final activo = fechaSalida == null;
                      return Card(
                        child: ListTile(
                          leading: CircleAvatar(
                            backgroundColor: activo ? Colors.green : Colors.grey,
                            child: const Icon(Icons.directions_car, color: Colors.white),
                          ),
                          title: Text('${r['matricula'] ?? ''} - ${r['nombreCompleto'] ?? ''}',
                              style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: Text(
                            '${r['documentoIdentidad'] ?? ''}\n'
                            'Ingreso: ${r['fechaIngreso']?.toString().substring(0, 19) ?? ''}'
                            '${fechaSalida != null ? '\nSalida: ${fechaSalida.toString().substring(0, 19)}' : '  [Activo]'}'
                            '\n${r['puertaAcceso'] ?? ''}'
                            '\nAprobado por: ${r['registradoPorNombre'] ?? '-'}',
                          ),
                          isThreeLine: true,
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
