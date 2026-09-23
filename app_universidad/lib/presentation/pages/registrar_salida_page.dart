import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../../core/network/api_client.dart';
import '../../core/theme/app_theme.dart';
import '../widgets/foto_ingreso.dart';
import '../widgets/udi_appbar_logo.dart';
import '../../data/datasources/sistema_remote_data_source.dart';

class RegistrarSalidaPage extends StatefulWidget {
  const RegistrarSalidaPage({super.key});

  @override
  State<RegistrarSalidaPage> createState() => _RegistrarSalidaPageState();
}

class _RegistrarSalidaPageState extends State<RegistrarSalidaPage> {
  final _searchController = TextEditingController();
  final _nombreController = TextEditingController();
  final _ciController = TextEditingController();
  final _horaController = TextEditingController();

  int? _registroId;

  /// Referencia `sharepoint:{uid}` de la foto tomada al ingresar, si la hay.
  String? _fotoEntrada;

  bool _buscando = false;
  bool _registrando = false;

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

  Future<void> _buscarRegistro() async {
    FocusScope.of(context).unfocus();
    final ci = _searchController.text.trim();
    if (ci.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Por favor, escriba un C.I. primero')),
      );
      return;
    }

    setState(() {
      _buscando = true;
      _registroId = null;
    });

    final api = context.read<ApiClient>();
    try {
      final activos = await api.getList('/api/Porteria/activos');
      final encontrado = activos.cast<Map<String, dynamic>>().where(
        (v) => v['documentoIdentidad'] == ci,
      ).toList();

      if (encontrado.isNotEmpty) {
        final r = encontrado.first;
        setState(() {
          _nombreController.text = r['nombreCompleto'] ?? '';
          _ciController.text = r['documentoIdentidad'] ?? ci;
          _registroId = r['idRegistro'];
          _fotoEntrada = r['fotoUrl'] as String?;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Registro encontrado.'), backgroundColor: Colors.green),
        );
      } else {
        setState(() {
          _nombreController.clear();
          _ciController.text = ci;
          _fotoEntrada = null;
        });
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Sin registro activo.')),
        );
      }
    } catch (e) {
      setState(() => _ciController.text = ci);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error al buscar registro: $e')),
      );
    } finally {
      setState(() => _buscando = false);
    }
  }

  Future<void> _registrarSalida() async {
    setState(() => _registrando = true);
    final api = context.read<ApiClient>();
    try {
      await api.post('/api/Porteria/salida', {
        'id': _registroId ?? 0,
        'puertaSalida': 'Principal',
      });
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('¡Salida registrada exitosamente!'), backgroundColor: Colors.green),
        );
        Future.delayed(const Duration(milliseconds: 1500), () {
          if (mounted) Navigator.pop(context);
        });
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
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            const UdiAppBarLogo(width: 40),
            const SizedBox(width: 8),
            const Text('Registrar Salida', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
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
                const Text('Buscar registro para marcar salida',
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
                        textInputAction: TextInputAction.search,
                        onSubmitted: (_) => _buscarRegistro(),
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
                      onPressed: _buscando ? null : _buscarRegistro,
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
                const Text('Confirmación de Datos',
                    style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Colors.white)),
                const SizedBox(height: 15),
                // Foto tomada al entrar: permite verificar que quien sale es la
                // misma persona. Solo aparece si el registro la tiene.
                if (_fotoEntrada != null) ...[
                  Row(
                    children: [
                      FotoIngresoAvatar(
                        referencia: _fotoEntrada,
                        radio: 34,
                        titulo: _nombreController.text,
                      ),
                      const SizedBox(width: 12),
                      const Expanded(
                        child: Text(
                          'Foto registrada al ingresar.\nToca para ampliar.',
                          style: TextStyle(color: Colors.white70, fontSize: 12),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 15),
                ],
                TextField(
                  controller: _nombreController,
                  style: const TextStyle(color: Colors.black87),
                  cursorColor: Colors.black87,
                  decoration: _inputDecoration('Nombre Completo', Icons.person_outline),
                ),
                const SizedBox(height: 15),
                Row(
                  children: [
                    Expanded(
                      flex: 6,
                      child: TextField(
                        controller: _ciController,
                        keyboardType: TextInputType.number,
                        style: const TextStyle(color: Colors.black87),
                        cursorColor: Colors.black87,
                        decoration: _inputDecoration('C.I.', Icons.badge_outlined),
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
                        decoration: _inputDecoration('Hora Salida', Icons.access_time),
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 30),
                ElevatedButton(
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Colors.white,
                    foregroundColor: AppTheme.udiRed,
                    padding: const EdgeInsets.symmetric(vertical: 16),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30.0)),
                  ),
                  onPressed: _registrando ? null : _registrarSalida,
                  child: _registrando
                      ? const SizedBox(width: 24, height: 24, child: CircularProgressIndicator(strokeWidth: 2))
                      : const Text('CONFIRMAR SALIDA', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }
}
