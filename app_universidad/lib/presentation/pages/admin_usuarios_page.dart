import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/constants/error_messages.dart';
import '../../core/network/api_client.dart';
import '../../core/services/logger_service.dart';
import '../../core/theme/app_theme.dart';
import '../../core/widgets/error_view.dart';
import '../widgets/udi_appbar_logo.dart';
import 'crear_usuario_admin_page.dart';

class AdminUsuariosPage extends StatefulWidget {
  const AdminUsuariosPage({super.key});

  @override
  State<AdminUsuariosPage> createState() => _AdminUsuariosPageState();
}

class _AdminUsuariosPageState extends State<AdminUsuariosPage> {
  List<Map<String, dynamic>> _usuarios = [];
  bool _cargando = true;
  String? _error;

  @override
  void initState() {
    super.initState();
    _cargarUsuarios();
  }

  Future<void> _cargarUsuarios() async {
    setState(() {
      _cargando = true;
      _error = null;
    });
    final api = context.read<ApiClient>();
    try {
      final list = await api.getList('/api/Admin/usuarios');
      if (!mounted) return;
      setState(() => _usuarios = list.cast<Map<String, dynamic>>());
    } catch (e, st) {
      LoggerService.error('Al cargar los usuarios', e, st);
      if (!mounted) return;
      setState(() {
        _usuarios = [];
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
            const Text('Usuarios', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ],
        ),
        centerTitle: true,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh, color: Colors.white),
            onPressed: _cargarUsuarios,
          ),
          IconButton(
            icon: const Icon(Icons.person_add, color: Colors.white),
            onPressed: () async {
              await Navigator.push(
                context,
                MaterialPageRoute(builder: (context) => const CrearUsuarioAdminPage()),
              );
              _cargarUsuarios();
            },
          ),
        ],
      ),
      body: _cargando
          ? const Center(child: CircularProgressIndicator())
          : _error != null
              ? ErrorView(message: _error!, onRetry: _cargarUsuarios)
              : RefreshIndicator(
                  onRefresh: _cargarUsuarios,
                  child: ListView.builder(
                    padding: const EdgeInsets.all(10),
                    itemCount: _usuarios.isEmpty ? 1 : _usuarios.length,
                    itemBuilder: (context, index) {
                      if (_usuarios.isEmpty) {
                        return const Padding(
                          padding: EdgeInsets.all(24),
                          child: Center(
                            child: Text('Aún no hay usuarios registrados'),
                          ),
                        );
                      }
                      final u = _usuarios[index];
                      return Card(
                        child: ListTile(
                          leading: CircleAvatar(
                            backgroundColor: u['estado'] == true ? Colors.green : Colors.grey,
                            child: Text(
                              (u['nombreCompleto'] as String).substring(0, 1).toUpperCase(),
                              style: const TextStyle(color: Colors.white),
                            ),
                          ),
                          title: Text(u['nombreCompleto'] ?? '', style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: Text('${u['documentoIdentidad'] ?? ''}  |  ${u['rol'] ?? ''}  |  ${u['tipoUsuario'] ?? ''}'),
                          isThreeLine: false,
                        ),
                      );
                    },
                  ),
                ),
    );
  }
}
