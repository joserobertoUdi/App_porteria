import 'dart:io';

import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import 'package:provider/provider.dart';
import '../../core/network/api_client.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/udi_appbar_logo.dart';
import '../../data/datasources/sistema_remote_data_source.dart';

class RegistrarEntradaPage extends StatefulWidget {
  const RegistrarEntradaPage({super.key});

  @override
  State<RegistrarEntradaPage> createState() => _RegistrarEntradaPageState();
}

class _RegistrarEntradaPageState extends State<RegistrarEntradaPage> {
  final _searchController = TextEditingController();
  final _nombreController = TextEditingController();
  final _ciController = TextEditingController();
  final _motivoController = TextEditingController();
  final _horaController = TextEditingController();

  int _tipoUsuarioId = 3;
  String _puertaSeleccionada = 'Principal';
  final List<String> _puertas = ['Principal', 'Parqueo'];
  bool _buscando = false;
  bool _registrando = false;

  /// Foto capturada localmente. Se envía como parte del multipart al confirmar.
  final _picker = ImagePicker();
  File? _fotoLocal;

  final List<Map<String, dynamic>> _tiposUsuario = [
    {'id': 1, 'nombre': 'Estudiante'},
    {'id': 2, 'nombre': 'Trabajador'},
    {'id': 3, 'nombre': 'Visitante'},
  ];

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _obtenerHoraServidor());
  }

  Future<void> _obtenerHoraServidor() async {
    try {
      final hora = await context.read<SistemaRemoteDataSource>().obtenerHoraServidor();
      if (!mounted) return;
      _horaController.text = hora.horaFormateada;
    } catch (_) {
      if (!mounted) return;
      final ahora = DateTime.now();
      _horaController.text =
          '${ahora.hour.toString().padLeft(2, '0')}:${ahora.minute.toString().padLeft(2, '0')}';
    }
  }

  Future<void> _buscarUsuario() async {
    final dni = _searchController.text.trim();
    if (dni.isEmpty) return;

    setState(() {
      _buscando = true;
    });
    final api = context.read<ApiClient>();
    try {
      final response = await api.get('/api/Usuario/buscar/$dni');
      setState(() {
        _nombreController.text = response['nombreCompleto'] ?? '';
        _ciController.text = response['documentoIdentidad'] ?? dni;
        _tipoUsuarioId = response['tipoUsuarioId'] ?? 3;
      });
    } catch (_) {
      setState(() {
        _ciController.text = dni;
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Usuario no encontrado. Complete los datos manualmente.')),
        );
      }
    } finally {
      setState(() => _buscando = false);
    }
  }

  void _aviso(String mensaje, {bool esError = false}) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(
      content: Text(mensaje),
      backgroundColor: esError ? Colors.red[700] : Colors.green,
      duration: Duration(seconds: esError ? 7 : 3),
    ));
  }

  Future<void> _capturarFoto(ImageSource origen) async {
    final ci = _ciController.text.trim().isNotEmpty
        ? _ciController.text.trim()
        : _searchController.text.trim();

    if (ci.isEmpty) {
      _aviso('Ingresa primero el C.I.: la foto se archiva con ese dato.',
          esError: true);
      return;
    }

    final XFile? tomada;
    try {
      tomada = await _picker.pickImage(
        source: origen,
        imageQuality: 70,
        maxWidth: 1280,
        preferredCameraDevice: CameraDevice.rear,
      );
    } catch (e) {
      _aviso('No se pudo abrir ${origen == ImageSource.camera ? 'la cámara' : 'la galería'}: $e',
          esError: true);
      return;
    }

    if (tomada == null || !mounted) return;

    setState(() {
      _fotoLocal = File(tomada!.path);
    });
  }

  void _quitarFoto() {
    setState(() {
      _fotoLocal = null;
    });
  }

  Future<void> _registrarEntrada() async {
    final ci = _searchController.text.trim().isNotEmpty
        ? _searchController.text.trim()
        : _ciController.text.trim();
    if (ci.isEmpty || _nombreController.text.trim().isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Complete los datos obligatorios')),
      );
      return;
    }

    setState(() => _registrando = true);
    final api = context.read<ApiClient>();

    try {
      await api.postMultipart('/api/Porteria/entrada-completa-foto', fields: {
        'documentoIdentidad': ci,
        'nombreCompleto': _nombreController.text.trim(),
        'tipoUsuarioId': _tipoUsuarioId.toString(),
        'puertaEntrada': _puertaSeleccionada,
        'motivoVisita': _motivoController.text.trim(),
        'areaDestino': '',
      }, file: _fotoLocal, fileFieldName: 'foto');

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Entrada registrada'), backgroundColor: Colors.green),
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
  void dispose() {
    _searchController.dispose();
    _nombreController.dispose();
    _ciController.dispose();
    _motivoController.dispose();
    _horaController.dispose();
    super.dispose();
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

  Widget _buildSeccionFoto() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(14),
        border: Border.all(color: Colors.white30),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.photo_camera_outlined, color: Colors.white, size: 20),
              const SizedBox(width: 8),
              const Text(
                'Foto del ingreso',
                style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white),
              ),
              const Spacer(),
              const Text('Opcional', style: TextStyle(fontSize: 12, color: Colors.white70)),
            ],
          ),
          const SizedBox(height: 12),
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _buildVistaPrevia(),
              const SizedBox(width: 14),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    OutlinedButton.icon(
                      onPressed: () => _capturarFoto(ImageSource.camera),
                      icon: const Icon(Icons.photo_camera, size: 18),
                      label: const Text('Tomar foto'),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: Colors.white,
                        side: const BorderSide(color: Colors.white70),
                        padding: const EdgeInsets.symmetric(vertical: 12),
                      ),
                    ),
                    const SizedBox(height: 8),
                    OutlinedButton.icon(
                      onPressed: () => _capturarFoto(ImageSource.gallery),
                      icon: const Icon(Icons.photo_library_outlined, size: 18),
                      label: const Text('Cargar imagen'),
                      style: OutlinedButton.styleFrom(
                        foregroundColor: Colors.white,
                        side: const BorderSide(color: Colors.white70),
                        padding: const EdgeInsets.symmetric(vertical: 12),
                      ),
                    ),
                    if (_fotoLocal != null) ...[
                      const SizedBox(height: 8),
                      TextButton.icon(
                        onPressed: _quitarFoto,
                        icon: const Icon(Icons.close, size: 16),
                        label: const Text('Quitar foto'),
                        style: TextButton.styleFrom(foregroundColor: Colors.white70),
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          _buildEstadoFoto(),
        ],
      ),
    );
  }

  Widget _buildVistaPrevia() {
    const lado = 110.0;

    return ClipRRect(
      borderRadius: BorderRadius.circular(10),
      child: Container(
        width: lado,
        height: lado,
        color: Colors.white24,
        child: _fotoLocal == null
            ? const Icon(Icons.person_outline, size: 44, color: Colors.white70)
            : Image.file(_fotoLocal!, fit: BoxFit.cover),
      ),
    );
  }

  Widget _buildEstadoFoto() {
    if (_fotoLocal != null) {
      return Row(
        children: [
          const Icon(Icons.check_circle_outline, size: 15, color: Colors.greenAccent),
          const SizedBox(width: 6),
          Expanded(
            child: Text(
              'Foto capturada. Se subirá al confirmar.',
              style: TextStyle(fontSize: 12, color: Colors.greenAccent),
            ),
          ),
        ],
      );
    }
    return Row(
      children: [
        const Icon(Icons.info_outline, size: 15, color: Colors.white60),
        const SizedBox(width: 6),
        Expanded(
          child: Text(
            'Sin foto. Se registrará igualmente.',
            style: TextStyle(fontSize: 12, color: Colors.white60),
          ),
        ),
      ],
    );
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
            const Text('Registrar Entrada', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
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
            padding: const EdgeInsets.all(20.0),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text('Búsqueda rápida por registro (C.I.)',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
                const SizedBox(height: 10),
                Row(
                  children: [
                    Expanded(
                      child: TextField(
                        controller: _searchController,
                        keyboardType: TextInputType.number,
                        style: const TextStyle(color: Colors.black87),
                        cursorColor: Colors.black87,
                        decoration: _inputDecoration('Buscar por C.I.', Icons.search),
                      ),
                    ),
                    const SizedBox(width: 10),
                    ElevatedButton(
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Colors.white,
                        foregroundColor: AppTheme.udiRed,
                        padding: const EdgeInsets.symmetric(vertical: 15, horizontal: 15),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10.0)),
                      ),
                      onPressed: _buscando ? null : _buscarUsuario,
                      child: _buscando
                          ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                          : const Icon(Icons.person_search),
                    ),
                  ],
                ),
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 20.0),
                  child: Divider(color: Colors.white54),
                ),
                const Text('Datos de Ingreso',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
                const SizedBox(height: 15),
                TextField(
                  controller: _nombreController,
                  style: const TextStyle(color: Colors.black87),
                  cursorColor: Colors.black87,
                  decoration: _inputDecoration('Nombre Completo', Icons.person_outline),
                ),
                const SizedBox(height: 15),
                TextField(
                  controller: _ciController,
                  keyboardType: TextInputType.number,
                  style: const TextStyle(color: Colors.black87),
                  cursorColor: Colors.black87,
                  decoration: _inputDecoration('C.I.', Icons.badge_outlined),
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
                    Expanded(
                      flex: 6,
                      child: DropdownButtonFormField<String>(
                        value: _puertaSeleccionada,
                        style: const TextStyle(color: Colors.black87),
                        dropdownColor: Colors.white,
                        iconEnabledColor: Colors.grey,
                        decoration: _inputDecoration('Acceso', Icons.door_front_door_outlined),
                        items: _puertas.map((p) => DropdownMenuItem(value: p, child: Text(p))).toList(),
                        onChanged: (v) => setState(() => _puertaSeleccionada = v!),
                      ),
                    ),
                    const SizedBox(width: 10),
                    Expanded(
                      flex: 4,
                      child: TextField(
                        controller: _horaController,
                        readOnly: true,
                        style: const TextStyle(color: Colors.black87, fontWeight: FontWeight.bold),
                        cursorColor: Colors.black87,
                        decoration: _inputDecoration('Hora', Icons.access_time),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 15),
                TextField(
                  controller: _motivoController,
                  maxLines: 3,
                  style: const TextStyle(color: Colors.black87),
                  cursorColor: Colors.black87,
                  decoration: _inputDecoration('Motivo', Icons.edit_note),
                ),
                const SizedBox(height: 20),
                _buildSeccionFoto(),
                const SizedBox(height: 30),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white,
                    foregroundColor: AppTheme.udiRed,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30.0)),
                  ),
                  onPressed: _registrando ? null : _registrarEntrada,
                  child: _registrando
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text('CONFIRMAR ENTRADA', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
