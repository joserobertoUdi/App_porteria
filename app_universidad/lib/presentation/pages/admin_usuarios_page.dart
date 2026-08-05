import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/network/api_client.dart';
import '../../core/theme/app_theme.dart';
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

  @override
  void initState() {
    super.initState();
    _cargarUsuarios();
  }

  Future<void> _cargarUsuarios() async {
    setState(() => _cargando = true);
    final api = context.read<ApiClient>();
    try {
      final list = await api.getList('/api/Admin/usuarios');
      setState(() => _usuarios = list.cast<Map<String, dynamic>>());
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Error al cargar usuarios')),
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
        title: const Text('Usuarios', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        centerTitle: true,
        actions: [
          const UdiAppBarLogo(),
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
          : RefreshIndicator(
              onRefresh: _cargarUsuarios,
              child: ListView.builder(
                padding: const EdgeInsets.all(10),
                itemCount: _usuarios.length,
                itemBuilder: (context, index) {
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
