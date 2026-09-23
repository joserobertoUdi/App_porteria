import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/network/api_client.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/udi_appbar_logo.dart';

class CrearUsuarioAdminPage extends StatefulWidget {
  const CrearUsuarioAdminPage({super.key});

  @override
  State<CrearUsuarioAdminPage> createState() => _CrearUsuarioAdminPageState();
}

class _CrearUsuarioAdminPageState extends State<CrearUsuarioAdminPage> {
  final _nombreController = TextEditingController();
  final _dniController = TextEditingController();
  final _passwordController = TextEditingController();

  int _tipoUsuarioId = 3;
  int _rolId = 3;
  bool _registrando = false;

  @override
  void dispose() {
    _nombreController.dispose();
    _dniController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _crearUsuario() async {
    if (_nombreController.text.trim().isEmpty ||
        _dniController.text.trim().isEmpty ||
        _passwordController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Todos los campos son obligatorios')),
      );
      return;
    }

    setState(() => _registrando = true);
    final api = context.read<ApiClient>();
    try {
      await api.post('/api/Admin/usuarios', {
        'nombreCompleto': _nombreController.text.trim(),
        'documentoIdentidad': _dniController.text.trim(),
        'password': _passwordController.text,
        'tipoUsuarioId': _tipoUsuarioId,
        'rolId': _rolId,
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Usuario creado'), backgroundColor: Colors.green),
        );
        Navigator.pop(context);
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Error: ${e.toString().replaceFirst('Exception: ', '')}')),
        );
      }
    } finally {
      setState(() => _registrando = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const UdiAppBarLogo(width: 40),
            const SizedBox(width: 8),
            const Text('Crear Usuario', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
          ],
        ),
        iconTheme: const IconThemeData(color: Colors.white),
      ),
      body: Container(
        width: double.infinity,
        height: double.infinity,
        decoration: const BoxDecoration(gradient: AppTheme.udiGradient),
        child: SafeArea(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(20),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                _campoTexto(_nombreController, 'Nombre completo', Icons.person),
                const SizedBox(height: 15),
                _campoTexto(_dniController, 'Carnet de identidad', Icons.badge_outlined),
                const SizedBox(height: 15),
                _campoTexto(_passwordController, 'Contraseña', Icons.fingerprint, obscure: true),
                const SizedBox(height: 15),
                DropdownButtonFormField<int>(
                  value: _tipoUsuarioId,
                  style: const TextStyle(color: Colors.white),
                  dropdownColor: AppTheme.udiRed,
                  iconEnabledColor: Colors.white,
                  decoration: _inputDecoration('Tipo de persona', Icons.person_outline),
                  items: const [
                    DropdownMenuItem(value: 1, child: Text('Estudiante')),
                    DropdownMenuItem(value: 2, child: Text('Trabajador')),
                    DropdownMenuItem(value: 3, child: Text('Visitante')),
                  ],
                  onChanged: (v) => setState(() => _tipoUsuarioId = v!),
                ),
                const SizedBox(height: 15),
                DropdownButtonFormField<int>(
                  value: _rolId,
                  style: const TextStyle(color: Colors.white),
                  dropdownColor: AppTheme.udiRed,
                  iconEnabledColor: Colors.white,
                  decoration: _inputDecoration('Rol de sistema', Icons.admin_panel_settings),
                  items: const [
                    DropdownMenuItem(value: 1, child: Text('Administrador')),
                    DropdownMenuItem(value: 2, child: Text('Portero Parqueo')),
                    DropdownMenuItem(value: 3, child: Text('Portero Portería')),
                  ],
                  onChanged: (v) => setState(() => _rolId = v!),
                ),
                const SizedBox(height: 30),
                ElevatedButton(
                  onPressed: _registrando ? null : _crearUsuario,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white,
                    foregroundColor: AppTheme.udiRed,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30)),
                  ),
                  child: _registrando
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text('CREAR USUARIO', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _campoTexto(TextEditingController ctrl, String label, IconData icon, {bool obscure = false}) {
    return TextField(
      controller: ctrl,
      obscureText: obscure,
      style: const TextStyle(color: Colors.white),
      decoration: _inputDecoration(label, icon),
    );
  }

  InputDecoration _inputDecoration(String label, IconData icon) {
    return InputDecoration(
      labelText: label,
      labelStyle: const TextStyle(color: Colors.white70),
      prefixIcon: Icon(icon, color: Colors.white),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: const BorderSide(color: Colors.white70),
      ),
      focusedBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(10),
        borderSide: const BorderSide(color: Colors.white, width: 2),
      ),
    );
  }
}
