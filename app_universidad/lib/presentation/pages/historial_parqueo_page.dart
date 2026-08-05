import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/network/api_client.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/udi_appbar_logo.dart';

class HistorialParqueoPage extends StatefulWidget {
  const HistorialParqueoPage({super.key});

  @override
  State<HistorialParqueoPage> createState() => _HistorialParqueoPageState();
}

class _HistorialParqueoPageState extends State<HistorialParqueoPage> {
  List<Map<String, dynamic>> _registros = [];
  bool _cargando = true;

  @override
  void initState() {
    super.initState();
    _cargarHistorial();
  }

  Future<void> _cargarHistorial() async {
    setState(() => _cargando = true);
    final api = context.read<ApiClient>();
    try {
      final list = await api.getList('/api/Admin/parqueo/historial');
      setState(() => _registros = list.cast<Map<String, dynamic>>());
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Error al cargar historial')),
        );
      }
    } finally {
      setState(() => _cargando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: const Text('Historial Parqueo', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        centerTitle: true,
        actions: [
          const UdiAppBarLogo(),
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _cargarHistorial,
          ),
        ],
      ),
      body: _cargando
          ? const Center(child: CircularProgressIndicator())
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
                            '\n${r['puertaAcceso'] ?? ''}',
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
