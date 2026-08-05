import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/udi_appbar_logo.dart';
import '../../data/datasources/parqueo_remote_data_source.dart';
import '../../data/models/parqueo_models.dart';

class ParqueosActivosPage extends StatefulWidget {
  const ParqueosActivosPage({super.key});

  @override
  State<ParqueosActivosPage> createState() => _ParqueosActivosPageState();
}

class _ParqueosActivosPageState extends State<ParqueosActivosPage> {
  List<ParqueoActivoModel> _activos = [];
  bool _cargando = true;

  @override
  void initState() {
    super.initState();
    _cargarActivos();
  }

  Future<void> _cargarActivos() async {
    setState(() => _cargando = true);
    final api = context.read<ParqueoRemoteDataSource>();
    try {
      final lista = await api.obtenerActivos();
      setState(() => _activos = lista);
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Error al cargar parqueos activos')),
        );
      }
    } finally {
      setState(() => _cargando = false);
    }
  }

  Future<void> _cerrarSalida(int idRegistro) async {
    final api = context.read<ParqueoRemoteDataSource>();
    try {
      await api.registrarSalida(idRegistro);
      _cargarActivos();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Salida registrada'), backgroundColor: Colors.green),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: ${e.toString().replaceFirst('Exception: ', '')}')),
        );
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        backgroundColor: AppTheme.udiRed,
        title: const Text('Parqueos Activos', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        centerTitle: true,
        actions: [
          const UdiAppBarLogo(),
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _cargarActivos,
          ),
        ],
      ),
      body: _cargando
          ? const Center(child: CircularProgressIndicator())
          : _activos.isEmpty
              ? const Center(child: Text('No hay parqueos activos', style: TextStyle(fontSize: 16)))
              : RefreshIndicator(
                  onRefresh: _cargarActivos,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(10),
                    itemCount: _activos.length,
                    itemBuilder: (context, index) {
                      final item = _activos[index];
                      return Card(
                        margin: const EdgeInsets.symmetric(vertical: 5),
                        child: ListTile(
                          leading: const CircleAvatar(
                            backgroundColor: Colors.green,
                            child: Icon(Icons.directions_car, color: Colors.white),
                          ),
                          title: Text(item.matricula, style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: Text(
                            '${item.nombreCompleto}\n'
                            'Ingreso: ${item.fechaIngreso.toString().substring(0, 19)}',
                          ),
                          trailing: IconButton(
                            icon: const Icon(Icons.logout, color: AppTheme.udiRed),
                            onPressed: () => _cerrarSalida(item.idRegistro),
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
