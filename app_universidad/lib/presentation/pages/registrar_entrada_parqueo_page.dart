import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/udi_appbar_logo.dart';
import '../../core/utils/uppercase_text_formatter.dart';
import '../../data/datasources/parqueo_remote_data_source.dart';

class RegistrarEntradaParqueoPage extends StatefulWidget {
  const RegistrarEntradaParqueoPage({super.key});

  @override
  State<RegistrarEntradaParqueoPage> createState() => _RegistrarEntradaParqueoPageState();
}

class _RegistrarEntradaParqueoPageState extends State<RegistrarEntradaParqueoPage> {
  final _carnetController = TextEditingController();
  final _nombreController = TextEditingController();
  final _placaController = TextEditingController();
  final _marcaController = TextEditingController();
  final _modeloController = TextEditingController();
  final _colorController = TextEditingController();
  final _observacionesController = TextEditingController();

  int _tipoUsuarioId = 3;
  String _puertaSeleccionada = 'Principal';
  final _puertas = ['Principal', 'Parqueo Este', 'Parqueo Oeste'];

  bool _buscando = false;
  bool _registrando = false;

  final List<Map<String, dynamic>> _tiposUsuario = [
    {'id': 1, 'nombre': 'Estudiante'},
    {'id': 2, 'nombre': 'Trabajador'},
    {'id': 3, 'nombre': 'Visitante'},
  ];

  @override
  void dispose() {
    _carnetController.dispose();
    _nombreController.dispose();
    _placaController.dispose();
    _marcaController.dispose();
    _modeloController.dispose();
    _colorController.dispose();
    _observacionesController.dispose();
    super.dispose();
  }

  Future<void> _buscarUsuario() async {
    final dni = _carnetController.text.trim();
    if (dni.isEmpty) return;

    setState(() => _buscando = true);

    final api = context.read<ParqueoRemoteDataSource>();
    try {
      final response = await api.client.get('/api/Usuario/buscar/$dni');
      setState(() {
        _nombreController.text = response['nombreCompleto'] ?? '';
        _tipoUsuarioId = response['tipoUsuarioId'] ?? 3;
      });

      final usuarioId = response['id'] as int?;
      if (usuarioId != null) {
        final vehiculos = await api.obtenerVehiculosPorUsuario(usuarioId);
        if (vehiculos.isNotEmpty) {
          final v = vehiculos.first;
          _placaController.text = (v['matricula'] ?? '').toString().toUpperCase();
          _marcaController.text = (v['marca'] ?? '').toString();
          _modeloController.text = (v['modelo'] ?? '').toString();
          _colorController.text = (v['color'] ?? '').toString();
        }
      }
    } catch (_) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Usuario no encontrado. Ingrese datos manualmente.')),
        );
      }
    } finally {
      setState(() => _buscando = false);
    }
  }

  Future<void> _registrarEntrada() async {
    if (_carnetController.text.trim().isEmpty || _placaController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Carnet y placa son obligatorios')),
      );
      return;
    }

    setState(() => _registrando = true);

    final api = context.read<ParqueoRemoteDataSource>();
    try {
      await api.registrarEntradaCompleta(
        documentoIdentidad: _carnetController.text.trim(),
        nombreCompleto: _nombreController.text.trim().isNotEmpty ? _nombreController.text.trim() : null,
        tipoUsuarioId: _tipoUsuarioId.toString(),
        matricula: _placaController.text.trim().toUpperCase(),
        marca: _marcaController.text.trim().isNotEmpty ? _marcaController.text.trim() : null,
        modelo: _modeloController.text.trim().isNotEmpty ? _modeloController.text.trim() : null,
        color: _colorController.text.trim().isNotEmpty ? _colorController.text.trim() : null,
        puertaAcceso: _puertaSeleccionada,
        observaciones: _observacionesController.text.trim().isNotEmpty ? _observacionesController.text.trim() : null,
      );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Entrada de parqueo registrada'), backgroundColor: Colors.green),
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
      if (mounted) setState(() => _registrando = false);
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
            const Text('Entrada Parqueo', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
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
                const Text('Buscar por carnet de identidad',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _carnetController,
                        style: const TextStyle(color: Colors.black87),
                        decoration: _inputDecoration('Carnet', Icons.badge_outlined),
                      ),
                    ),
                    const SizedBox(width: 10),
                    ElevatedButton(
                      onPressed: _buscando ? null : _buscarUsuario,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: AppTheme.udiRed,
                        padding: const EdgeInsets.symmetric(vertical: 15, horizontal: 15),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: _buscando
                          ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Icon(Icons.person_search),
                    ),
                  ],
                ),
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20),
                  child: Divider(color: Colors.white54),
                ),
                const Text('Datos del vehículo',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
                const SizedBox(height: 15),
                TextField(
                  controller: _nombreController,
                  style: const TextStyle(color: Colors.black87),
                  decoration: _inputDecoration('Nombre', Icons.person_outline),
                ),
                const SizedBox(height: 15),
                TextField(
                  controller: _placaController,
                  textCapitalization: TextCapitalization.characters,
                  inputFormatters: [UppercaseTextFormatter()],
                  style: const TextStyle(color: Colors.black87),
                  decoration: _inputDecoration('Placa', Icons.directions_car),
                ),
                const SizedBox(height: 15),
                DropdownButtonFormField<int>(
                  value: _tipoUsuarioId,
                  style: const TextStyle(color: Colors.black87),
                  dropdownColor: Colors.white,
                  iconEnabledColor: Colors.grey,
                  decoration: _inputDecoration('Tipo de Usuario', Icons.category_outlined),
                  items: _tiposUsuario.map((t) => DropdownMenuItem(
                    value: t['id'] as int,
                    child: Text(t['nombre'] as String),
                  )).toList(),
                  onChanged: (v) {
                    if (v != null) setState(() => _tipoUsuarioId = v);
                  },
                ),
                const SizedBox(height: 15),
                Row(
                  children: [
                    Expanded(child: TextField(
                      controller: _marcaController,
                      style: const TextStyle(color: Colors.black87),
                      decoration: _inputDecoration('Marca', Icons.build_outlined),
                    )),
                    const SizedBox(width: 10),
                    Expanded(child: TextField(
                      controller: _modeloController,
                      style: const TextStyle(color: Colors.black87),
                      decoration: _inputDecoration('Modelo', Icons.build_outlined),
                    )),
                  ],
                ),
                const SizedBox(height: 15),
                Row(
                  children: [
                    Expanded(
                      child: DropdownButtonFormField<String>(
                        value: _puertaSeleccionada,
                        style: const TextStyle(color: Colors.black87),
                        dropdownColor: Colors.white,
                        iconEnabledColor: Colors.grey,
                        decoration: _inputDecoration('Puerta', Icons.door_front_door_outlined),
                        items: _puertas.map((p) => DropdownMenuItem(value: p, child: Text(p))).toList(),
                        onChanged: (v) => setState(() => _puertaSeleccionada = v!),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(child: TextField(
                      controller: _colorController,
                      style: const TextStyle(color: Colors.black87),
                      decoration: _inputDecoration('Color', Icons.palette_outlined),
                    )),
                  ],
                ),
                const SizedBox(height: 15),
                TextField(
                  controller: _observacionesController,
                  maxLines: 2,
                  style: const TextStyle(color: Colors.black87),
                  decoration: _inputDecoration('Observaciones', Icons.edit_note),
                ),
                const SizedBox(height: 30),
                ElevatedButton(
                  onPressed: _registrando ? null : _registrarEntrada,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white,
                    foregroundColor: AppTheme.udiRed,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30)),
                  ),
                  child: _registrando
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text('REGISTRAR ENTRADA', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  InputDecoration _inputDecoration(String label, IconData icon) {
    return InputDecoration(
      hintText: label,
      hintStyle: TextStyle(color: Colors.grey[600]),
      filled: true,
      fillColor: Colors.white,
      prefixIcon: Icon(icon, color: Colors.grey[600]),
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
