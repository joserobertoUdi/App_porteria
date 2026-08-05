import 'package:flutter/material.dart';
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
      final ahora = hora.fechaHoraLocal;
      _horaController.text =
          '${ahora.hour.toString().padLeft(2, '0')}:${ahora.minute.toString().padLeft(2, '0')}';
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
      await api.post('/api/Porteria/entrada-completa', {
        'documentoIdentidad': ci,
        'nombreCompleto': _nombreController.text.trim(),
        'tipoUsuarioId': _tipoUsuarioId,
        'puertaEntrada': _puertaSeleccionada,
        'motivoVisita': _motivoController.text.trim(),
        'areaDestino': '',
      });
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

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      extendBodyBehindAppBar: true,
      appBar: AppBar(
        backgroundColor: Colors.transparent,
        elevation: 0,
        title: const Text('Registrar Entrada', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
        iconTheme: const IconThemeData(color: Colors.white),
        actions: const [UdiAppBarLogo()],
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
